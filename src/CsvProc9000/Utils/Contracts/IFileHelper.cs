using System.Text;

namespace CsvProc9000.Utils.Contracts;

internal interface IFileHelper
{
    Encoding DetectEncodingOfFile(string fileName);
    Encoding GetEncodingFromCharsetString(string charset);
}