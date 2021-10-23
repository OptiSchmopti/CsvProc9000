using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using CsvProc9000.Csv;
using CsvProc9000.Options;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CsvProc9000.Processors
{
    internal sealed class CsvProcessor : ICsvProcessor
    {
        private readonly ILogger<CsvProcessor> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly ICsvReader _csvReader;
        private readonly CsvProcessorOptions _processorOptions;

        public CsvProcessor(
            [NotNull] ILogger<CsvProcessor> logger,
            [NotNull] IOptions<CsvProcessorOptions> processorOptions,
            [NotNull] IFileSystem fileSystem,
            [NotNull] ICsvReader csvReader)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processorOptions = processorOptions.Value ?? throw new ArgumentNullException(nameof(processorOptions));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _csvReader = csvReader ?? throw new ArgumentNullException(nameof(csvReader));
        }

        public async Task ProcessAsync(IFileInfo file)
        {
            _logger.LogDebug("Processor: Waiting until file {File} can be processed", file.FullName);
            await WaitUntilFileIsUnlockedAsync(file);

            if (!CanProcess(file)) return;

            await ProcessInternalAsync(file);
        }

        private bool CanProcess(IFileInfo file)
        {
            if (!file.Exists)
            {
                _logger.LogDebug("Cannot process file {File}, because it does not exist", file.FullName);
                return false;
            }

            if (file.IsReadOnly)
            {
                _logger.LogDebug("Cannot process file {File}, because it is read-only", file.FullName);
                return false;
            }

            // ReSharper disable once InvertIf
            if (!file.Extension.Equals(".csv"))
            {
                _logger.LogDebug(
                    "Cannot process file {File}, because it has file-extensions '{Extension}' instead of '.csv'",
                    file.FullName, file.Extension);
                return false;
            }

            return true;
        }

        private async Task ProcessInternalAsync(IFileSystemInfo file)
        {
            _logger.LogInformation("Processor: Starting to process {File}...", file.FullName);

            _logger.LogDebug("Processor: Reading in file {File}...", file.FullName);
            var csvFile = await _csvReader.ReadAsync(file.FullName, _processorOptions.InboxDelimiter);

            _logger.LogDebug("Processor: Applying rules to file {File}...", file.FullName);
            ApplyRulesToFile(csvFile);
        }
        
        //     if (_fileSystem.Directory.Exists(_processorOptions.Outbox))
        //         _fileSystem.Directory.CreateDirectory(_processorOptions.Outbox);

        private void ApplyRulesToFile(CsvFile csvFile)
        {
            if (_processorOptions.Rules == null || !_processorOptions.Rules.Any())
            {
                _logger.LogWarning("Processor: Cannot process file {File} because there are no rules defined",
                    csvFile.FileName);
                return;
            }

            foreach (var rule in _processorOptions.Rules)
                ApplyRuleToFile(csvFile, rule);
        }

        private void ApplyRuleToFile(CsvFile csvFile, Rule rule)
        {
            if (rule.Conditions == null || !rule.Conditions.Any())
            {
                _logger.LogWarning("Processor: Skipping Rule at index {Index} because it has no conditions",
                    _processorOptions.Rules.IndexOf(rule));
                return;
            }

            foreach (var row in csvFile.Rows)
                ApplyRuleToRow(row, rule, csvFile);
        }

        private void ApplyRuleToRow(CsvRow row, Rule rule, CsvFile file)
        {
            if (!MeetsRowConditions(row, rule, file)) return;

            _logger.LogTrace("Processor: File {File} meets rule at index {RuleIndex}. Applying rule...",
                file.FileName, _processorOptions.Rules.IndexOf(rule));
            
            foreach (var (columnName, fieldValue) in rule.Steps)
            {
                _logger.LogTrace("Processor: Row at index {RowIndex}: Adding field '{Field}' with value '{FieldValue}'",
                    _processorOptions.Rules.IndexOf(rule), columnName, fieldValue);
                row.AddOrUpdateField(columnName, fieldValue);
            }
        }

        private bool MeetsRowConditions(CsvRow row, Rule rule, CsvFile file)
        {
            foreach (var condition in rule.Conditions)
            {
                var field = row.Fields.FirstOrDefault(field => field.Column.Name == condition.Field);
                if (field == null)
                {
                    _logger.LogTrace(
                        "Processor: Row at index {RowIndex} does not meet condition at index {ConditionIndex}, " +
                        "because the field '{FieldName}' could not be found",
                        file.Rows.ToList().IndexOf(row), rule.Conditions.IndexOf(condition), condition.Field);
                    return false;
                }

                if (field.Value == condition.Value) continue;
                
                _logger.LogTrace(
                    "Processor: Row at index {RowIndex} does not meet condition at index {ConditionIndex}, " + 
                    "because the field '{FieldName}' with the value '{FieldValue}' " +
                    "does not have the desired value '{ConditionValue}'",
                    file.Rows.ToList().IndexOf(row), rule.Conditions.IndexOf(condition), condition.Field,
                    field.Value, condition.Value);
                return false;
            }
            
            // if we get here, all conditions have been met
            return true;
        }

        private async Task WaitUntilFileIsUnlockedAsync(IFileSystemInfo file)
        {
            while (IsFileLocked(file))
            {
                await Task.Delay(TimeSpan.FromSeconds(0.1));
            }
        }

        private bool IsFileLocked(IFileSystemInfo file)
        {
            // HACKy way of checking if the file is still locked
            try
            {
                using var stream =
                    _fileSystem.File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

                // if this succeeds the file is not locked
                return false;
            }
            catch (IOException)
            {
                // the file is unavailable because it is:
                // - still being written to
                // - being processed by another thread
                // - does not exist (has already been processed)
                return true;
            }
        }
    }
}