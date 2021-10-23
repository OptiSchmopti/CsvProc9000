using System.Threading.Tasks;

namespace CsvProc9000.Csv
{
    public interface ICsvReader
    {
        Task<CsvFile> ReadAsync(string fileName, string delimiter);
    }
}