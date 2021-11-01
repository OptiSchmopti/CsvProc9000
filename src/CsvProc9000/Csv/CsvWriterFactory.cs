using CsvHelper;
using CsvHelper.Configuration;
using CsvProc9000.Csv.Contracts;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;

namespace CsvProc9000.Csv
{
    [ExcludeFromCodeCoverage] // simple factory
    internal sealed class CsvWriterFactory : ICsvWriterFactory
    {
        public IWriter Create(string file, string delimiter)
        {
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = delimiter };

            var streamWriter = new StreamWriter(file);
            var writer = new CsvWriter(streamWriter, csvConfiguration);
            return writer;
        }
    }
}
