using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace CsvProc9000.Model.Csv
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
    }
}
