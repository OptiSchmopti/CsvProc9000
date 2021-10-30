using System.Threading.Tasks;
using CsvProc9000.Csv;

namespace CsvProc9000.Processors
{
    public interface ISaveCsvFile
    {
        Task SaveToAsync(
            CsvFile file, 
            string destinationFileName, 
            string delimiter,
            bool fieldValuesWrappedInQuotes);
    }
}
