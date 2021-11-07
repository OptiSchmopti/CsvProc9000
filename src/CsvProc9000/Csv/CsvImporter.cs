using CsvHelper;
using CsvProc9000.Csv.Contracts;
using CsvProc9000.Model;
using CsvProc9000.Model.Csv;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CsvProc9000.Csv
{
    internal sealed class CsvImporter : ICsvImporter
    {
        private readonly ICsvReaderFactory _readerFactory;

        public CsvImporter([NotNull] ICsvReaderFactory readerFactory)
        {
            _readerFactory = readerFactory ?? throw new ArgumentNullException(nameof(readerFactory));
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
            catch (IOException ex)
            {
                return new Result<CsvFile>(false, failureMessage: ex.Message);
            }
        }

        private async Task<CsvFile> DoImportAsync(
            string fileName,
            string delimiter)
        {
            using var reader = _readerFactory.Create(fileName, delimiter);

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
