using CsvProc9000.Csv;

namespace CsvProc9000.Processors
{
    public interface IApplyRulesToCsvFile
    {
        void Apply(CsvFile csvFile);
    }
}