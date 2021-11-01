using CsvProc9000.Jobs.Contracts;
using CsvProc9000.Options;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CsvProc9000.BackgroundServices
{
    internal sealed class CsvProcessJobThreadSpawnerBackgroundService : BackgroundService
    {
        private readonly CsvProcessorOptions _csvProcessorOptions;

        private readonly List<IDisposable> _disposables = new();
        private readonly ICsvProcessJobThreadFactory _jobThreadFactory;

        public CsvProcessJobThreadSpawnerBackgroundService(
            [NotNull] IOptions<CsvProcessorOptions> csvProcessorOptions,
            [NotNull] ICsvProcessJobThreadFactory jobThreadFactory)
        {
            _jobThreadFactory = jobThreadFactory ?? throw new ArgumentNullException(nameof(jobThreadFactory));
            _csvProcessorOptions = csvProcessorOptions.Value ??
                                   throw new ArgumentNullException(nameof(csvProcessorOptions));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_csvProcessorOptions.JobThreadCount <= 0)
                throw new ArgumentException(
                    $"'{nameof(_csvProcessorOptions.JobThreadCount)}' has to be a positive number above 0!");

            for (var index = 0; index < _csvProcessorOptions.JobThreadCount; index++)
            {
                var jobThread = _jobThreadFactory.Create();
                jobThread.Start(stoppingToken);
                _disposables.Add(jobThread);
            }

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (var disposable in _disposables)
                disposable?.Dispose();
        }
    }
}
