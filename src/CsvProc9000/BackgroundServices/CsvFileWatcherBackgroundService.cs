using CsvProc9000.Jobs;
using CsvProc9000.Jobs.Contracts;
using CsvProc9000.Model.Configuration;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace CsvProc9000.BackgroundServices
{
    internal sealed class CsvFileWatcherBackgroundService : BackgroundService
    {
        private readonly IFileSystem _fileSystem;
        private readonly IFileSystemWatcher _fileSystemWatcher;
        private readonly IJobPool _jobPool;
        private readonly ILogger<CsvFileWatcherBackgroundService> _logger;
        private readonly CsvProcessorOptions _processorOptions;

        public CsvFileWatcherBackgroundService(
            [NotNull] ILogger<CsvFileWatcherBackgroundService> logger,
            [NotNull] IOptions<CsvProcessorOptions> processorOptions,
            [NotNull] IFileSystem fileSystem,
            [NotNull] IJobPool jobPool)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _jobPool = jobPool ?? throw new ArgumentNullException(nameof(jobPool));
            _processorOptions = processorOptions.Value ?? throw new ArgumentNullException(nameof(processorOptions));

            _fileSystemWatcher = fileSystem.FileSystemWatcher.CreateNew(_processorOptions.Inbox);
            _fileSystemWatcher.Filter = "*.csv";

            _fileSystemWatcher.Created += OnFileCreated;
            _fileSystemWatcher.Renamed += OnFileRenamed;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_fileSystem.Directory.Exists(_processorOptions.Inbox))
                _fileSystem.Directory.CreateDirectory(_processorOptions.Inbox);

            _logger.LogInformation("FileWatcher: Starting to watch for files in {Target}...", _processorOptions.Inbox);
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
            _fileSystemWatcher.Renamed -= OnFileRenamed;
            _fileSystemWatcher.Dispose();
        }

        private void OnFileCreated(object sender, FileSystemEventArgs eventArgs)
        {
            if (!eventArgs.ChangeType.HasFlag(WatcherChangeTypes.Created)) return;

            _logger.LogDebug("FileWatcher: File Created: {File}", eventArgs.FullPath);
            AddJobForFile(eventArgs.FullPath);
        }

        private void OnFileRenamed(object sender, RenamedEventArgs eventArgs)
        {
            if (!eventArgs.ChangeType.HasFlag(WatcherChangeTypes.Renamed)) return;
            
            _logger.LogDebug("FileWatcher: File Renamed: from {OldFile} to {File}", 
                eventArgs.OldFullPath, eventArgs.FullPath);
            AddJobForFile(eventArgs.FullPath);
        }

        private void AddJobForFile(string filePath)
        {
            var file = _fileSystem.FileInfo.FromFileName(filePath);
            var job = new CsvProcessJob(file);

            _logger.LogDebug("FileWatcher: Added CsvProcessJob #{JobId} for {File}", job.Id, filePath);

            _jobPool.Add(job);
        }
    }
}
