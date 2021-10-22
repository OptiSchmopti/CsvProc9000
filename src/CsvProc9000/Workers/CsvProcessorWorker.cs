using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using CsvProc9000.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CsvProc9000.Workers
{
    public class CsvProcessorWorker : BackgroundService
    {
        private readonly ILogger<CsvProcessorWorker> _logger;
        private readonly CsvProcessorOptions _processorOptions;
        private readonly IFileSystemWatcher _fileSystemWatcher;

        public CsvProcessorWorker(
            ILogger<CsvProcessorWorker> logger, 
            IOptions<CsvProcessorOptions> processorOptions,
            IFileSystem fileSystem)
        {
            if (fileSystem == null) throw new ArgumentNullException(nameof(fileSystem));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processorOptions = processorOptions?.Value ?? throw new ArgumentNullException(nameof(processorOptions));

            _fileSystemWatcher = fileSystem.FileSystemWatcher.CreateNew(_processorOptions.WatchTarget);
            // _fileSystemWatcher.Filter = "*.csv";
            
            _fileSystemWatcher.Changed += OnChanged;
            _fileSystemWatcher.Created += OnCreated;
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

        /*
         * NOTE from the Documentation of the FileSystemWatcher
         * 
         * The 'OnCreated' event is raised as soon as a file is created.
         * If a file is being copied or transferred into a watched directory,
         * the 'OnCreated' event will be raised immediately, followed by one or more OnChanged events.
         */
        
        private void OnCreated(object sender, FileSystemEventArgs eventArgs)
        {
            if (eventArgs.ChangeType != WatcherChangeTypes.Created) return;
            
            _logger.LogInformation("Created: {File}", eventArgs.FullPath);
        }

        private void OnChanged(object sender, FileSystemEventArgs eventArgs)
        {
            if (eventArgs.ChangeType != WatcherChangeTypes.Changed) return;
            
            _logger.LogInformation("Changed: {File}", eventArgs.FullPath);
        }
    }
}