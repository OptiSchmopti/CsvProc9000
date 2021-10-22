using System.IO.Abstractions;
using System.Threading.Tasks;

namespace CsvProc9000.Processors
{
    public interface ICsvProcessor
    {
        Task ProcessAsync(IFileInfo file);
    }
}