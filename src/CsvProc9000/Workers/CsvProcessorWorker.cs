using System;
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
        private readonly IOptions<CsvProcessorOptions> _processorOptions;

        public CsvProcessorWorker(ILogger<CsvProcessorWorker> logger, IOptions<CsvProcessorOptions> processorOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processorOptions = processorOptions ?? throw new ArgumentNullException(nameof(processorOptions));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {Time}, options: {Watch}, {Output}", DateTimeOffset.Now,
                    _processorOptions.Value.WatchTarget, _processorOptions.Value.Output);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}