using CsvHelper;

namespace CsvProc9000.Csv.Contracts
{
    internal interface ICsvWriterFactory
    {
        IWriter Create(string file, string delimiter, string charset);
    }
}
