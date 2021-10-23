using System;
using System.IO;
using System.IO.Abstractions;
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
        private readonly IApplyRulesToCsvFile _applyRulesToCsvFile;
        private readonly ISaveCsvFile _saveCsvFile;
        private readonly CsvProcessorOptions _processorOptions;

        public CsvProcessor(
            [NotNull] ILogger<CsvProcessor> logger,
            [NotNull] IOptions<CsvProcessorOptions> processorOptions,
            [NotNull] IFileSystem fileSystem,
            [NotNull] ICsvReader csvReader,
            [NotNull] IApplyRulesToCsvFile applyRulesToCsvFile,
            [NotNull] ISaveCsvFile saveCsvFile)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processorOptions = processorOptions.Value ?? throw new ArgumentNullException(nameof(processorOptions));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _csvReader = csvReader ?? throw new ArgumentNullException(nameof(csvReader));
            _applyRulesToCsvFile = applyRulesToCsvFile ?? throw new ArgumentNullException(nameof(applyRulesToCsvFile));
            _saveCsvFile = saveCsvFile ?? throw new ArgumentNullException(nameof(saveCsvFile));
        }

        public async Task ProcessAsync(IFileInfo file)
        {
            _logger.LogDebug("Processor: Waiting until file {File} can be processed", file.FullName);
            await WaitUntilFileIsUnlockedAsync(file);

            if (!CanProcess(file)) return;

            await ProcessInternalAsync(file);
        }

        private bool CanProcess(IFileSystemInfo file)
        {
            if (!file.Exists)
            {
                _logger.LogDebug("Cannot process file {File}, because it does not exist", file.FullName);
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
            
            _applyRulesToCsvFile.Apply(csvFile);
            
            await SaveResultAsync(file, csvFile);
        }

        private async Task SaveResultAsync(IFileSystemInfo file, CsvFile csvFile)
        {
            var fileName = file.Name;
            var destinationFileName = _fileSystem.Path.Combine(_processorOptions.Outbox, fileName);
            
            _logger.LogInformation("Processor: Saving result to {Destination}...", destinationFileName);
            
            if (_fileSystem.Directory.Exists(_processorOptions.Outbox))
                _fileSystem.Directory.CreateDirectory(_processorOptions.Outbox);

            await _saveCsvFile.SaveToAsync(csvFile, destinationFileName, _processorOptions.OutboxDelimiter);

            if (!file.Exists) return;
            if (!_processorOptions.DeleteInboxFile) return;
            
            _logger.LogDebug("Processor: Deleting original file {File} from Inbox {Inbox}...",
                file.FullName, _processorOptions.Inbox);
            
            _fileSystem.File.Delete(file.FullName);
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