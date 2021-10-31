using System;
using System.Threading.Tasks;

namespace CsvProc9000.Jobs.Contracts
{
    internal interface ICsvProcessJobWorker
    {
        Task WorkOnAsync(Guid jobThreadId, CsvProcessJob job);
    }
}
