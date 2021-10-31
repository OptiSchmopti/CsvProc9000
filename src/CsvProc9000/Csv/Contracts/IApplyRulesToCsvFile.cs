using CsvProc9000.Model.Csv;
using System;

namespace CsvProc9000.Csv.Contracts
{
    public interface IApplyRulesToCsvFile
    {
        void Apply(CsvFile csvFile, Guid jobId, Guid jobThreadId);
    }
}
