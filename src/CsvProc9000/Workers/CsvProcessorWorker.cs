using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
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
        private readonly IFileSystem _fileSystem;
        private readonly CsvProcessorOptions _processorOptions;
        private readonly IFileSystemWatcher _fileSystemWatcher;

        private readonly List<IFileSystemInfo> _waitToCopyFiles = new();

        public CsvProcessorWorker(
            ILogger<CsvProcessorWorker> logger, 
            IOptions<CsvProcessorOptions> processorOptions,
            IFileSystem fileSystem)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
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
         *
         * Which basically means that on 'OnCreated' the file might still be accessed because it's still being copied
         * or written to, so we have to wait until that is finished
         */
        
        private void OnCreated(object sender, FileSystemEventArgs eventArgs)
        {
            if (eventArgs.ChangeType != WatcherChangeTypes.Created) return;
            
            _logger.LogDebug("Created: {File}", eventArgs.FullPath);

            var file = _fileSystem.FileInfo.FromFileName(eventArgs.FullPath);
            CopyToOutput(file);
        }

        private void OnChanged(object sender, FileSystemEventArgs eventArgs)
        {
            if (eventArgs.ChangeType != WatcherChangeTypes.Changed) return;
            
            _logger.LogDebug("Changed: {File}", eventArgs.FullPath);
            
            var waitToCopyCandidate = _waitToCopyFiles.FirstOrDefault(file => file.FullName == eventArgs.FullPath);
            if (waitToCopyCandidate == null) return;
            
            CopyToOutput(waitToCopyCandidate);
        }

        private void CopyToOutput(IFileSystemInfo file)
        {
            _logger.LogDebug("Attempting to copy file {File}", file.FullName);
            
            var fileName = file.Name;
            var outputCombined = _fileSystem.Path.Combine(_processorOptions.Output, fileName);

            try
            {
                _fileSystem.File.Copy(file.FullName, outputCombined, true);
                _logger.LogInformation("Copied file {File} to {Target}", file.FullName, _processorOptions.Output);

                if (_waitToCopyFiles.Contains(file))
                    _waitToCopyFiles.Remove(file);
            }
            catch (IOException e)
            {
                _logger.LogDebug("File could not be copied (Reason: '{Reason}'), waiting until it can be copied", e.Message);
                _waitToCopyFiles.Add(file);
            }
        }
    }
}