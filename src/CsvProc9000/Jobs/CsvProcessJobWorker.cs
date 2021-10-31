using CsvProc9000.Csv.Contracts;
using CsvProc9000.Jobs.Contracts;
using CsvProc9000.Model.Csv;
using CsvProc9000.Options;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace CsvProc9000.Jobs
{
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
            _csvProcessorOptions = csvProcessorOptions.Value ??
                                   throw new ArgumentNullException(nameof(csvProcessorOptions));
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

            _applyRulesToCsvFile.Apply(file, job.Id, jobThreadId);

            var exported = await ExportAsync(file, job, jobThreadId);

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
            while (csvFile == null)
            {
                var result =
                    await _csvImporter.ImportAsync(job.TargetFile.FullName, _csvProcessorOptions.InboxDelimiter);

                if (result.IsSuccess)
                    csvFile = result.Value;
            }

            return csvFile;
        }

        private async Task<bool> ExportAsync(
            CsvFile file,
            CsvProcessJob job,
            Guid jobThreadId)
        {
            var fileName = job.TargetFile.Name;
            var destinationFileName = _fileSystem.Path.Combine(_csvProcessorOptions.Outbox, fileName);

            try
            {
                _logger.LogInformation("T-{ThreadId} J-{JobId}# Exporting to {Destination}...",
                    jobThreadId, job.Id, destinationFileName);

                await _csvExporter.ExportAsync(
                    file, destinationFileName,
                    _csvProcessorOptions.OutboxDelimiter,
                    _csvProcessorOptions.OutboxValuesInQuotes);

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "T-{ThreadId} J-{JobId}# Export to {Destination} failed!",
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
}
