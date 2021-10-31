﻿using CsvProc9000.Jobs.Contracts;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CsvProc9000.Jobs
{
    internal sealed class CsvProcessJobThread : ICsvProcessJobThread
    {
        private readonly IJobPool _jobPool;
        private readonly ILogger<CsvProcessJobThread> _logger;
        private readonly ICsvProcessJobWorker _worker;

        public CsvProcessJobThread(
            [NotNull] IJobPool jobPool,
            [NotNull] ILogger<CsvProcessJobThread> logger,
            [NotNull] ICsvProcessJobWorker worker)
        {
            _jobPool = jobPool ?? throw new ArgumentNullException(nameof(jobPool));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _worker = worker ?? throw new ArgumentNullException(nameof(worker));
        }

        public Guid ThreadId { get; } = Guid.NewGuid();

        public void Start(CancellationToken stoppingToken)
        {
            _logger.LogTrace("Started Job-Thread {Id}...", ThreadId);

            Task.Run(() => ProcessThread(stoppingToken), CancellationToken.None);
        }

        private async Task ProcessThread(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
                await ProcessAsync();
        }

        private async Task ProcessAsync()
        {
            try
            {
                if (!_jobPool.TryGet<CsvProcessJob>(out var job)) return;

                IndicateExecutionStart(job);
                await _worker.WorkOnAsync(ThreadId, job);
                IndicateExecutionFinish(job);
            }
            finally
            {
                await Task.Delay(300);
            }
        }

        private void IndicateExecutionStart(CsvProcessJob job)
        {
            job.IndicateExecutionStart();

            _logger.LogDebug("T-{ThreadId} J-{JobId}# Start Work on {JobFile}...",
                ThreadId, job.Id, job.TargetFile);
        }

        private void IndicateExecutionFinish(CsvProcessJob job)
        {
            job.IndicateExecutionFinish();

            _logger.LogDebug("T-{ThreadId} J-{JobId}# Finish Work on {JobFile}",
                ThreadId, job.Id, job.TargetFile);

            _logger.LogDebug("T-{ThreadId} J-{JobId}# Job-Information: Created At {JobCreated}, " +
                             "Started At {JobStarted}, Finished At {JobFinished}",
                ThreadId, job.Id, job.Creation, job.ExecutionStart, job.ExecutionFinish);
        }
    }
}
