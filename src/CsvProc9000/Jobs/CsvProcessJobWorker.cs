using CsvProc9000.Csv.Contracts;
using CsvProc9000.Jobs.Contracts;
using CsvProc9000.Model.Configuration;
using CsvProc9000.Model.Csv;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace CsvProc9000.Jobs;

internal sealed class CsvProcessJobWorker : ICsvProcessJobWorker
{
    private readonly IApplyRulesToCsvFile _applyRulesToCsvFile;
    private readonly ICsvExporter _csvExporter;
    private readonly ICsvImporter _csvImporter;
    private readonly CsvProcessorOptions _csvProcessorOptions;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<CsvProcessJobWorker> _logger;

    public CsvProcessJobWorker(
        [NotNull] IOptions<CsvProcessorOptions> csvProcessorOptions,
        [NotNull] ILogger<CsvProcessJobWorker> logger,
        [NotNull] ICsvImporter csvImporter,
        [NotNull] IApplyRulesToCsvFile applyRulesToCsvFile,
        [NotNull] ICsvExporter csvExporter,
        [NotNull] IFileSystem fileSystem)
    {
        _csvProcessorOptions = csvProcessorOptions.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _csvImporter = csvImporter ?? throw new ArgumentNullException(nameof(csvImporter));
        _applyRulesToCsvFile = applyRulesToCsvFile ?? throw new ArgumentNullException(nameof(applyRulesToCsvFile));
        _csvExporter = csvExporter ?? throw new ArgumentNullException(nameof(csvExporter));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task WorkOnAsync(Guid jobThreadId, CsvProcessJob job)
    {
        if (!CanProcess(job, jobThreadId)) return;

        var file = await ImportFileAsync(job, jobThreadId);

        var outboxRule = DetermineFittingOutboxRule(file, job, jobThreadId);

        _applyRulesToCsvFile.Apply(file, job.Id, jobThreadId);

        var exported = await ExportAsync(outboxRule, file, job, jobThreadId);

        if (exported)
            DeleteOriginal(job, jobThreadId);
    }

    private bool CanProcess(CsvProcessJob job, Guid jobThreadId)
    {
        if (!job.TargetFile.Exists)
        {
            _logger.LogDebug("T-{ThreadId} J-{JobId}# Cannot process file {File}, because it does not exist",
                jobThreadId, job.Id, job.TargetFile);
            return false;
        }

        // ReSharper disable once InvertIf
        if (!job.TargetFile.Extension.Equals(".csv"))
        {
            _logger.LogDebug("T-{ThreadId} J-{JobId}# Cannot process file {File}, " +
                             "because it has file-extensions '{Extension}' instead of '.csv'",
                jobThreadId, job.Id, job.TargetFile, job.TargetFile.Extension);
            return false;
        }

        return true;
    }

    private async Task<CsvFile> ImportFileAsync(CsvProcessJob job, Guid jobThreadId)
    {
        _logger.LogInformation("T-{ThreadId} J-{JobId}# Importing {JobFile}...",
            jobThreadId, job.Id, job.TargetFile);

        /*
         * NOTE from the Documentation of the FileSystemWatcher
         *
         * The 'Created' event is raised as soon as a file is created.
         * If a file is being copied or transferred into a watched directory,
         * the 'Created' event will be raised immediately, followed by one or more 'Changed' events.
         *
         * Which basically means that on 'Created' the file might still be accessed and not completely written to
         * because it's still being copied or written to, so we have to wait until that is finished
         */
        CsvFile csvFile = null;
        // TODO: I have to rethink this thing... it's weird...
        while (csvFile == null)
        {
            var result = await _csvImporter.ImportAsync(job.TargetFile.FullName, _csvProcessorOptions.InboxDelimiter);

            if (result.IsSuccess)
                csvFile = result.Value;
            else
            {
                _logger.LogWarning("T-{ThreadId} J-{JobId}# Import failed, because of '{Message}'. Retrying after 500ms...", jobThreadId, job.Id, result.FailureMessage);
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
        }

        return csvFile;
    }

    private OutboxRule DetermineFittingOutboxRule(CsvFile file, CsvProcessJob job, Guid jobThreadId)
    {
        var outboxRules = _csvProcessorOptions.OutboxRules;

        var fallback = outboxRules.FirstOrDefault(rule => rule.Conditions == null || !rule.Conditions.Any());
        var outboxRulesWithoutFallback = outboxRules.Where(rule => rule != fallback);

        var fittingOutboxRule = outboxRulesWithoutFallback
            .Select(outboxRule => new
            {
                outboxRule,
                anyRowMeetsConditions = file.Rows.Any(row => row.MeetsConditions(outboxRule.Conditions)),
            })
            .Where(x => x.anyRowMeetsConditions)
            .Select(x => x.outboxRule).FirstOrDefault();

        // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
        if (fittingOutboxRule == null)
        {
            _logger.LogInformation("T-{ThreadId} J-{JobId}# Unable to determine outbox from conditions. Trying to use fallback '{OutboxName}' {Fallback}...", jobThreadId, fallback?.Name, job.Id, fallback?.Outbox);
            fittingOutboxRule = fallback ?? throw new InvalidOperationException($"Could not determine outbox for file '{job.TargetFile.FullName}' and no fallback defined!");
        }

        _logger.LogInformation("T-{ThreadId} J-{JobId}# Determined outbox to be '{OutboxName}' at {Destination}...", jobThreadId, job.Id, fittingOutboxRule.Name, fittingOutboxRule.Outbox);

        return fittingOutboxRule;
    }

    private async Task<bool> ExportAsync(
        OutboxRule outboxRule,
        CsvFile file,
        CsvProcessJob job,
        Guid jobThreadId)
    {
        var fileName = job.TargetFile.Name;
        var destinationFileName = _fileSystem.Path.Combine(outboxRule.Outbox, fileName);

        try
        {
            _logger.LogInformation("T-{ThreadId} J-{JobId}# Exporting to {Destination}...",
                jobThreadId, job.Id, destinationFileName);

            await _csvExporter.ExportAsync(
                file,
                destinationFileName,
                outboxRule.OutboxDelimiter,
                outboxRule.OutboxFileCharset,
                outboxRule.OutboxValuesInQuotes);

            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "T-{ThreadId} J-{JobId}# Export to {Destination} failed!",
                jobThreadId, job.Id, destinationFileName);

            if (_fileSystem.File.Exists(destinationFileName))
                _fileSystem.File.Delete(destinationFileName);
            return false;
        }
    }

    private void DeleteOriginal(CsvProcessJob job, Guid jobThreadId)
    {
        if (!job.TargetFile.Exists) return;
        if (!_csvProcessorOptions.DeleteInboxFile) return;

        _logger.LogDebug("T-{ThreadId} J-{JobId}# Deleting original file {File} from Inbox {Inbox}",
            jobThreadId, job.Id, job.TargetFile, _csvProcessorOptions.Inbox);

        job.TargetFile.Delete();
    }
}
