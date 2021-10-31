using CsvProc9000.Model.Csv;
using JetBrains.Annotations;
using System.Threading.Tasks;

namespace CsvProc9000.Csv.Contracts
{
    public interface ICsvExporter
    {
        Task ExportAsync(
            [NotNull] CsvFile file,
            [NotNull] string destinationFileName,
            [NotNull] string delimiter,
            bool wrapValuesInQuotes = false);
    }
}
