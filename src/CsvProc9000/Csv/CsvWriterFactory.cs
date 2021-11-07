using CsvHelper;
using CsvHelper.Configuration;
using CsvProc9000.Csv.Contracts;
using CsvProc9000.Utils.Contracts;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;

namespace CsvProc9000.Csv
{
    [ExcludeFromCodeCoverage] // simple factory
    internal sealed class CsvWriterFactory : ICsvWriterFactory
    {
        private readonly IFileHelper _fileHelper;

        public CsvWriterFactory([NotNull] IFileHelper fileHelper)
        {
            _fileHelper = fileHelper ?? throw new ArgumentNullException(nameof(fileHelper));
        }
        
        public IWriter Create(string file, string delimiter, string charset)
        {
            var encoding = _fileHelper.GetEncodingFromCharsetString(charset);
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter,
                Encoding = encoding
            };

            var streamWriter = new StreamWriter(file, false, encoding);
            var writer = new CsvWriter(streamWriter, csvConfiguration);
            return writer;
        }
    }
}
