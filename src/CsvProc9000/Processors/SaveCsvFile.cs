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
            string delimiter,
            bool fieldValuesWrappedInQuotes)
        {
            var contentStringBuilder = new StringBuilder();

            var columns = DetermineColumns(file);
            AddHeaderRow(contentStringBuilder, delimiter, columns, fieldValuesWrappedInQuotes);
            AddRows(contentStringBuilder, file, delimiter, columns, fieldValuesWrappedInQuotes);

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
            IEnumerable<CsvColumn> columns,
            bool fieldValuesWrappedInQuotes)
        {
            contentStringBuilder.AppendJoin(delimiter, columns.Select(column => 
                fieldValuesWrappedInQuotes ? $"\"{column.Name}\"" : column.Name));
            contentStringBuilder.AppendLine();
        }

        private static void AddRows(
            StringBuilder contentStringBuilder, 
            CsvFile file, 
            string delimiter, 
            List<CsvColumn> columns,
            bool fieldValuesWrappedInQuotes)
        {
            foreach (var row in file.Rows)
            {
                AddRow(contentStringBuilder, delimiter, columns, row, fieldValuesWrappedInQuotes);
                contentStringBuilder.AppendLine();
            }
        }

        private static void AddRow(
            StringBuilder contentStringBuilder, 
            string delimiter, 
            List<CsvColumn> columns, 
            CsvRow row,
            bool fieldValuesWrappedInQuotes)
        {
            var firstIteration = true;

            foreach (var column in columns)
            {
                // append the delimiter to the previous field when get here not in the first iteration
                if (firstIteration) firstIteration = false;
                else contentStringBuilder.Append(delimiter);

                AddField(contentStringBuilder, row, column, fieldValuesWrappedInQuotes);
            }
        }

        private static void AddField(
            StringBuilder contentStringBuilder, 
            CsvRow row, 
            CsvColumn column,
            bool fieldValuesWrappedInQuotes)
        {
            var field = row.Fields.FirstOrDefault(f => f.Column == column);
            var fieldValue = string.Empty;

            if (field != null)
                fieldValue = fieldValuesWrappedInQuotes ? $"\"{field.Value}\"" : field.Value;

            contentStringBuilder.Append(fieldValue);
        }
    }
}
