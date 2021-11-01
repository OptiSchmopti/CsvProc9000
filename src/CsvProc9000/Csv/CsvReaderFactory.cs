using CsvHelper;
using CsvHelper.Configuration;
using CsvProc9000.Csv.Contracts;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;

namespace CsvProc9000.Csv
{
    [ExcludeFromCodeCoverage] // simple factory
    internal sealed class CsvReaderFactory : ICsvReaderFactory
    {
        private readonly IFileSystem _fileSystem;

        public CsvReaderFactory(
            [JetBrains.Annotations.NotNull] IFileSystem fileSystem)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public IReader Create(string file, string delimiter)
        {
            /*
             * NOTE
             * We're using FileShare.None here, so that no other process can modify the file while we're reading it
             * and additionally we'll be 'notified' when another process is still writing to a file
             */
            var fileStream = _fileSystem.FileStream.Create(file, FileMode.Open, FileAccess.Read, FileShare.None);
            var streamReader = new StreamReader(fileStream);

            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter, HasHeaderRecord = true
            };
            var reader = new CsvReader(streamReader, csvConfiguration);

            return reader;
        }
    }
}
