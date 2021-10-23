using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CsvProc9000.Csv
{
    public class CsvFile
    {
        private readonly List<CsvRow> _rows = new();

        public CsvFile([NotNull] string fileName)
        {
            OriginalFileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }
        
        public string OriginalFileName { get; }

        public IEnumerable<CsvRow> Rows => _rows;

        public void AddRow([NotNull] CsvRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            
            _rows.Add(row);
        }

        public async Task SaveToAsync(
            IFileSystem fileSystem, 
            string destinationFileName, 
            string delimiter)
        {
            var contentStringBuilder = new StringBuilder();

            var fieldNames = Rows
                .SelectMany(row => row.Fields)
                .Select(field => field.Name)
                .Distinct()
                .ToList();

            // add header row
            contentStringBuilder.AppendJoin(delimiter, fieldNames);
            contentStringBuilder.AppendLine();

            foreach (var row in Rows)
            {
                var firstIteration = true;

                foreach (var fieldName in fieldNames)
                {
                    // append the delimiter to the previous field when get here not in the first iteration
                    if (firstIteration) firstIteration = false;
                    else contentStringBuilder.Append(delimiter);
                    
                    var field = row.Fields.FirstOrDefault(f => f.Name == fieldName);
                    var fieldValue = string.Empty;

                    if (field != null)
                        fieldValue = field.Value;

                    contentStringBuilder.Append(fieldValue);
                }

                contentStringBuilder.AppendLine();
            }

            var content = contentStringBuilder.ToString();
            await fileSystem.File.WriteAllTextAsync(destinationFileName, content);
        }
    }
}