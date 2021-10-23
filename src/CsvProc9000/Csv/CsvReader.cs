using System;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using JetBrains.Annotations;

namespace CsvProc9000.Csv
{
    public class CsvReader : ICsvReader
    {
        private readonly IFileSystem _fileSystem;

        public CsvReader(
            [NotNull] IFileSystem fileSystem)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }
        
        public async Task<CsvFile> ReadAsync(string fileName, string delimiter)
        {
            await using var fileStream = _fileSystem.FileStream.Create(fileName, FileMode.Open, FileAccess.Read);
            using var streamReader = new StreamReader(fileStream);

            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter,
                HasHeaderRecord = true
            };
            using var csvReader = new CsvHelper.CsvReader(streamReader, configuration);

            var csvFile = await CreateCsvFileFromHeader(fileName, csvReader);
            await ProcessCsvRows(csvReader, csvFile);

            return csvFile;
        }

        private static async Task<CsvFile> CreateCsvFileFromHeader(string fileName, IReader csvReader)
        {
            await csvReader.ReadAsync();
            if (!csvReader.ReadHeader())
                throw new InvalidOperationException($"Could not read header of file {fileName}");

            var header = csvReader.HeaderRecord;
            if (!header.Any())
                throw new InvalidOperationException($"Did not find any headers for file {fileName}");

            var csvFile = new CsvFile(header);
            return csvFile;
        }

        private static async Task ProcessCsvRows(IReader csvReader, CsvFile csvFile)
        {
            while (await csvReader.ReadAsync())
            {
                var row = ProcessCsvRow(csvReader, csvFile);
                csvFile.AddRow(row);
            }
        }

        private static CsvRow ProcessCsvRow(IReaderRow csvReader, CsvFile csvFile)
        {
            var csvRow = new CsvRow();

            foreach (var column in csvFile.Columns)
            {
                if (!csvReader.TryGetField<string>(column.Index, out var fieldValue)) continue;

                csvRow.AddField(column, fieldValue);
            }

            return csvRow;
        }
    }
}