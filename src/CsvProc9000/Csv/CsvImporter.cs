using CsvHelper;
using CsvHelper.Configuration;
using CsvProc9000.Csv.Contracts;
using CsvProc9000.Model;
using CsvProc9000.Model.Csv;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace CsvProc9000.Csv
{
    internal sealed class CsvImporter : ICsvImporter
    {
        private readonly IFileSystem _fileSystem;

        public CsvImporter(
            [NotNull] IFileSystem fileSystem)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public async Task<Result<CsvFile>> ImportAsync(
            string fileName,
            string delimiter)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileName));
            if (string.IsNullOrWhiteSpace(delimiter))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(delimiter));

            try
            {
                var file = await DoImportAsync(fileName, delimiter);
                return new Result<CsvFile>(true, file);
            }
            catch (IOException)
            {
                return new Result<CsvFile>(false);
            }
        }

        private async Task<CsvFile> DoImportAsync(
            string fileName,
            string delimiter)
        {
            // FileShare.None, so that no other process can use the file, while we're reading
            // and we're aware, when the file is still locked by another process
            await using var fileStream =
                _fileSystem.FileStream.Create(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
            using var streamReader = new StreamReader(fileStream);

            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter, HasHeaderRecord = true
            };
            using var reader = new CsvReader(streamReader, csvConfiguration);

            var file = new CsvFile(fileName);
            var headers = await GetHeadersAsync(fileName, reader);
            await ProcessRowsAsync(reader, file, headers);

            return file;
        }

        private static async Task<IEnumerable<string>> GetHeadersAsync(
            string fileName,
            IReader reader)
        {
            await reader.ReadAsync();
            if (!reader.ReadHeader())
                throw new InvalidOperationException($"Could not read header of file {fileName}");

            var headers = reader.HeaderRecord;
            if (!headers.Any())
                throw new InvalidOperationException($"Did not find any headers for file {fileName}");

            return headers;
        }

        private static async Task ProcessRowsAsync(
            IReader reader,
            CsvFile file,
            IEnumerable<string> headers)
        {
            var headerList = headers.ToList();

            while (await reader.ReadAsync())
            {
                var row = ProcessRow(reader, headerList);
                file.AddRow(row);
            }
        }

        private static CsvRow ProcessRow(
            IReaderRow reader,
            IReadOnlyList<string> headerList)
        {
            var row = new CsvRow();

            for (var index = 0; index < headerList.Count; index++)
            {
                if (!reader.TryGetField<string>(index, out var fieldValue)) continue;

                var column = new CsvColumn(index, headerList[index]);
                row.AddField(column, fieldValue);
            }

            return row;
        }
    }
}
