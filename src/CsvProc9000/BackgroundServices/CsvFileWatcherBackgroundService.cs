using CsvProc9000.Jobs;
using CsvProc9000.Jobs.Contracts;
using CsvProc9000.Options;
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
            _fileSystemWatcher.Dispose();
        }

        private void OnFileCreated(object sender, FileSystemEventArgs eventArgs)
        {
            if (eventArgs.ChangeType != WatcherChangeTypes.Created) return;

            _logger.LogDebug("FileWatcher: File Created: {File}", eventArgs.FullPath);

            var file = _fileSystem.FileInfo.FromFileName(eventArgs.FullPath);
            var job = new CsvProcessJob(file);

            _logger.LogDebug("FileWatcher: Added CsvProcessJob #{JobId}", job.Id);

            _jobPool.Add(job);
        }
    }
}
