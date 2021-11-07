using CsvHelper;
using CsvProc9000.Csv.Contracts;
using CsvProc9000.Model.Csv;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace CsvProc9000.Csv
{
    internal sealed class CsvExporter : ICsvExporter
    {
        private readonly IFileSystem _fileSystem;
        private readonly ICsvWriterFactory _writerFactory;

        public CsvExporter(
            [NotNull] IFileSystem fileSystem,
            [NotNull] ICsvWriterFactory writerFactory)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _writerFactory = writerFactory ?? throw new ArgumentNullException(nameof(writerFactory));
        }

        public async Task ExportAsync(
            CsvFile file,
            string destinationFileName,
            string delimiter,
            string charset,
            bool wrapValuesInQuotes = false)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (string.IsNullOrWhiteSpace(destinationFileName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(destinationFileName));
            if (string.IsNullOrWhiteSpace(delimiter))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(delimiter));
            if (string.IsNullOrWhiteSpace(charset))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(charset));

            MakeSureDirectoryExists(destinationFileName);

            await using var writer = _writerFactory.Create(destinationFileName, delimiter, charset);

            var columns = GetColumns(file).ToList();

            await WriteHeadersAsync(columns, writer, wrapValuesInQuotes);
            await WriteRowsAsync(file, columns, writer, wrapValuesInQuotes);

            await writer.FlushAsync();
        }

        private void MakeSureDirectoryExists(string destinationFileName)
        {
            var file = _fileSystem.FileInfo.FromFileName(destinationFileName);
            var directory = file.Directory;

            if (!directory.Exists)
                directory.Create();
        }

        private static async Task WriteHeadersAsync(
            IEnumerable<CsvColumn> columns,
            IWriter writer,
            bool wrapValuesInQuotes)
        {
            var headers = GetHeaders(columns);
            foreach (var header in headers)
                writer.WriteField(header, wrapValuesInQuotes);

            await writer.NextRecordAsync();
        }

        private static async Task WriteRowsAsync(
            CsvFile file,
            List<CsvColumn> columns,
            IWriter writer,
            bool wrapValuesInQuotes)
        {
            foreach (var row in file.Rows)
            {
                WriteRow(columns, writer, row, wrapValuesInQuotes);

                await writer.NextRecordAsync();
            }
        }

        private static void WriteRow(
            List<CsvColumn> columns,
            IWriterRow writer,
            CsvRow row,
            bool wrapValuesInQuotes)
        {
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var column in columns)
            {
                var field = row.Fields.FirstOrDefault(fld => fld.Column == column);

                writer.WriteField(field?.Value ?? string.Empty, wrapValuesInQuotes);
            }
        }

        private static IEnumerable<CsvColumn> GetColumns(CsvFile file)
        {
            var columns = file
                .Rows
                .SelectMany(row => row.Fields)
                .Select(field => field.Column)
                .Distinct()
                .OrderBy(column => column.Index)
                .ToList();

            return columns;
        }

        private static IEnumerable<string> GetHeaders(IEnumerable<CsvColumn> columns)
        {
            var headers = columns
                .Select(column => column.Name);

            return headers.ToList();
        }
    }
}
