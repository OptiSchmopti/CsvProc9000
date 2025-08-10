using CsvHelper;

namespace CsvProc9000.Csv.Contracts;

internal interface ICsvReaderFactory
{
    IReader Create(string file, string delimiter);
}