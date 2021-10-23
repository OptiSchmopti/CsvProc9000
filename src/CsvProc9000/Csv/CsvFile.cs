using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CsvProc9000.Csv
{
    public class CsvFile
    {
        private readonly List<CsvRow> _rows = new();

        public CsvFile([NotNull] string fileName)
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }
        
        public string FileName { get; }

        public IEnumerable<CsvRow> Rows => _rows;

        public void AddRow([NotNull] CsvRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            
            _rows.Add(row);
        }
    }
}