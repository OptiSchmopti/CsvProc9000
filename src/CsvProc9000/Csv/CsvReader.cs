using System;
using System.Collections.Generic;
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

            var csvFile = new CsvFile(fileName);
            var headers = await GetHeaders(fileName, csvReader);
            await ProcessCsvRows(csvReader, csvFile, headers);

            return csvFile;
        }

        private static async Task<IEnumerable<string>> GetHeaders(string fileName, IReader csvReader)
        {
            await csvReader.ReadAsync();
            if (!csvReader.ReadHeader())
                throw new InvalidOperationException($"Could not read header of file {fileName}");

            var headers = csvReader.HeaderRecord;
            if (!headers.Any())
                throw new InvalidOperationException($"Did not find any headers for file {fileName}");
            
            return headers;
        }

        private static async Task ProcessCsvRows(IReader csvReader, CsvFile csvFile, IEnumerable<string> headers)
        {
            var headersList = headers.ToList();
            while (await csvReader.ReadAsync())
            {
                var row = ProcessCsvRow(csvReader, headersList);
                csvFile.AddRow(row);
            }
        }

        private static CsvRow ProcessCsvRow(IReaderRow csvReader, IEnumerable<string> headers)
        {
            var csvRow = new CsvRow();

            var headersList = headers.ToList();
            for (var index = 0; index < headersList.Count; index++)
            {
                if (!csvReader.TryGetField<string>(index, out var fieldValue)) continue;
                
                var column = new CsvColumn(index, headersList[index]);
                csvRow.AddField(column, fieldValue);
            }

            return csvRow;
        }
    }
}