using CsvHelper;
using CsvHelper.Configuration;
using CsvProc9000.Csv.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using Ude;

namespace CsvProc9000.Csv
{
    [ExcludeFromCodeCoverage] // simple factory
    internal sealed class CsvReaderFactory : ICsvReaderFactory
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<CsvReaderFactory> _logger;

        public CsvReaderFactory(
            [NotNull] IFileSystem fileSystem,
            [NotNull] ILogger<CsvReaderFactory> logger)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IReader Create(string file, string delimiter)
        {
            /*
             * NOTE
             * We're using FileShare.None here, so that no other process can modify the file while we're reading it
             * and additionally we'll be 'notified' when another process is still writing to a file
             */
            var encoding = GetEncoding(file);
            var fileStream = _fileSystem.FileStream.Create(file, FileMode.Open, FileAccess.Read, FileShare.None);
            var streamReader = new StreamReader(fileStream, encoding);

            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter, 
                HasHeaderRecord = true,
                Encoding = encoding
            };
            var reader = new CsvReader(streamReader, csvConfiguration);

            return reader;
        }

        private Encoding GetEncoding(string fileName)
        {
            using var fileStream = _fileSystem.FileStream.Create(fileName, FileMode.Open, FileAccess.Read);

            _logger.LogTrace("Trying to detect charset of '{File}'...", fileName);
            
            var detector = new CharsetDetector();
            detector.Feed(fileStream);
            detector.DataEnd();
            var charset = detector.Charset;
            
            _logger.LogTrace("Found charset '{Charset}' with a confidence of {Confidence}", 
                charset, detector.Confidence);

            var encoding = CodePagesEncodingProvider.Instance.GetEncoding(charset);
            return encoding;
        }
    }
}
