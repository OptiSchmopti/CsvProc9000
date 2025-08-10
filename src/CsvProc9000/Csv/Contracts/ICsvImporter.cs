using CsvProc9000.Model;
using CsvProc9000.Model.Csv;
using JetBrains.Annotations;
using System.Threading.Tasks;

namespace CsvProc9000.Csv.Contracts;

public interface ICsvImporter
{
    Task<Result<CsvFile>> ImportAsync([NotNull] string fileName, [NotNull] string delimiter);
}