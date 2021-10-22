using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using CsvProc9000.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CsvProc9000.Processors
{
    internal sealed class CsvProcessor : ICsvProcessor
    {
        private readonly ILogger<CsvProcessor> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly CsvProcessorOptions _processorOptions;

        public CsvProcessor(
            ILogger<CsvProcessor> logger,
            IOptions<CsvProcessorOptions> processorOptions,
            IFileSystem fileSystem)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processorOptions = processorOptions?.Value ?? throw new ArgumentNullException(nameof(processorOptions));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
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
            
            // ReSharper disable once InvertIf
            if (file.IsReadOnly)
            {
                _logger.LogDebug("Cannot process file {File}, because it is read-only", file.FullName);
                return false;
            }
            
            return true;
        }

        private async Task ProcessInternalAsync(IFileInfo file)
        {
            _logger.LogInformation("Processor: Starting to process {File}...", file.FullName);

            // simulating work for now
            await Task.Delay(TimeSpan.FromSeconds(1));

            _logger.LogInformation("Processor: Moving file {File} to {Output}", file.FullName, _processorOptions.Output);

            var fileName = file.Name;
            var destinationFileName = _fileSystem.Path.Combine(_processorOptions.Output, fileName);
            file.MoveTo(destinationFileName, true);
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