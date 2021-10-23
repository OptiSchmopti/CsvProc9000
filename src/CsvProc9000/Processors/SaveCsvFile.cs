using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvProc9000.Csv;
using JetBrains.Annotations;

namespace CsvProc9000.Processors
{
    public class SaveCsvFile : ISaveCsvFile
    {
        private readonly IFileSystem _fileSystem;

        public SaveCsvFile(
            [NotNull] IFileSystem fileSystem)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }
        
        public async Task SaveToAsync(
            CsvFile file,
            string destinationFileName, 
            string delimiter)
        {
            var contentStringBuilder = new StringBuilder();

            var columns = DetermineColumns(file);
            AddHeaderRow(contentStringBuilder, delimiter, columns);
            AddRows(contentStringBuilder, file, delimiter, columns);

            var content = contentStringBuilder.ToString();
            await _fileSystem.File.WriteAllTextAsync(destinationFileName, content);
        }

        private static List<CsvColumn> DetermineColumns(CsvFile file)
        {
            var columns = file.Rows
                .SelectMany(row => row.Fields)
                .Select(field => field.Column)
                .Distinct()
                .OrderBy(column => column.Index)
                .ToList();
            return columns;
        }

        private static void AddHeaderRow(
            StringBuilder contentStringBuilder, 
            string delimiter, 
            IEnumerable<CsvColumn> columns)
        {
            contentStringBuilder.AppendJoin(delimiter, columns.Select(column => column.Name));
            contentStringBuilder.AppendLine();
        }

        private static void AddRows(
            StringBuilder contentStringBuilder, 
            CsvFile file, 
            string delimiter, 
            List<CsvColumn> columns)
        {
            foreach (var row in file.Rows)
            {
                AddRow(contentStringBuilder, delimiter, columns, row);
                contentStringBuilder.AppendLine();
            }
        }

        private static void AddRow(StringBuilder contentStringBuilder, string delimiter, List<CsvColumn> columns, CsvRow row)
        {
            var firstIteration = true;

            foreach (var column in columns)
            {
                // append the delimiter to the previous field when get here not in the first iteration
                if (firstIteration) firstIteration = false;
                else contentStringBuilder.Append(delimiter);

                AddField(contentStringBuilder, row, column);
            }
        }

        private static void AddField(StringBuilder contentStringBuilder, CsvRow row, CsvColumn column)
        {
            var field = row.Fields.FirstOrDefault(f => f.Column == column);
            var fieldValue = string.Empty;

            if (field != null)
                fieldValue = field.Value;

            contentStringBuilder.Append(fieldValue);
        }
    }
}