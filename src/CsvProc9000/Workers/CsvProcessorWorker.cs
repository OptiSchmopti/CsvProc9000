using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using CsvProc9000.Options;
using CsvProc9000.Processors;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nito.AsyncEx.Synchronous;

namespace CsvProc9000.Workers
{
    internal sealed class CsvProcessorWorker : BackgroundService
    {
        private readonly ILogger<CsvProcessorWorker> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly ICsvProcessor _csvProcessor;
        private readonly CsvProcessorOptions _processorOptions;
        private readonly IFileSystemWatcher _fileSystemWatcher;

        public CsvProcessorWorker(
            ILogger<CsvProcessorWorker> logger, 
            IOptions<CsvProcessorOptions> processorOptions,
            IFileSystem fileSystem,
            ICsvProcessor csvProcessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _csvProcessor = csvProcessor ?? throw new ArgumentNullException(nameof(csvProcessor));
            _processorOptions = processorOptions?.Value ?? throw new ArgumentNullException(nameof(processorOptions));

            _fileSystemWatcher = fileSystem.FileSystemWatcher.CreateNew(_processorOptions.WatchTarget);

            _fileSystemWatcher.Created += OnFileCreated;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting to watch for files in {Target}...", _processorOptions.WatchTarget);
            _fileSystemWatcher.EnableRaisingEvents = true;
            
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            return base.StopAsync(cancellationToken);
        }
        
        public override void Dispose()
        {
            base.Dispose();

            _fileSystemWatcher.Created -= OnFileCreated;
            _fileSystemWatcher.Dispose();
        }

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
        
        private void OnFileCreated(object sender, FileSystemEventArgs eventArgs)
        {
            if (eventArgs.ChangeType != WatcherChangeTypes.Created) return;
            
            _logger.LogDebug("Watcher: File Created: {File}", eventArgs.FullPath);

            var file = _fileSystem.FileInfo.FromFileName(eventArgs.FullPath);
            _csvProcessor.ProcessAsync(file).WaitAndUnwrapException();
        }
    }
}