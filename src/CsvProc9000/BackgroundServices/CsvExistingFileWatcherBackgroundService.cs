using CsvProc9000.Jobs;
using CsvProc9000.Jobs.Contracts;
using CsvProc9000.Model.Configuration;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace CsvProc9000.BackgroundServices
{
    internal sealed class CsvExistingFileWatcherBackgroundService : BackgroundService
    {
        private readonly IFileSystem _fileSystem;
        private readonly IJobPool _jobPool;
        private readonly ILogger<CsvExistingFileWatcherBackgroundService> _logger;
        private readonly CsvProcessorOptions _processorOptions;

        public CsvExistingFileWatcherBackgroundService(
            [NotNull] IFileSystem fileSystem,
            [NotNull] IOptions<CsvProcessorOptions> processorOptions,
            [NotNull] ILogger<CsvExistingFileWatcherBackgroundService> logger,
            [NotNull] IJobPool jobPool)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _processorOptions = processorOptions.Value ?? throw new ArgumentNullException(nameof(processorOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jobPool = jobPool ?? throw new ArgumentNullException(nameof(jobPool));
        }

        // basically just finding left-over files and adding them to the job-pool
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("ExistingFileWatcher: Looking for existing files upon starting...");

            AddExistingFilesToPool();

            return Task.CompletedTask;
        }

        private void AddExistingFilesToPool()
        {
            var existingFiles = _fileSystem.Directory.GetFiles(_processorOptions.Inbox, "*.csv");
            foreach (var existingFile in existingFiles)
                AddExistingFileToPool(existingFile);
        }

        private void AddExistingFileToPool(string existingFile)
        {
            _logger.LogDebug("ExistingFileWatcher: Found file {File}", existingFile);

            var file = _fileSystem.FileInfo.FromFileName(existingFile);
            var job = new CsvProcessJob(file);

            _jobPool.Add(job);

            _logger.LogDebug("ExistingFileWatcher: Added CsvProcessJob #{JobId}", job.Id);
        }
    }
}
