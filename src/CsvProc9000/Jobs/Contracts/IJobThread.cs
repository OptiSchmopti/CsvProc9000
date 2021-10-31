using System;
using System.Threading;

namespace CsvProc9000.Jobs.Contracts
{
    internal interface IJobThread
    {
        Guid ThreadId { get; }
        void Start(CancellationToken stoppingToken);
    }
}
