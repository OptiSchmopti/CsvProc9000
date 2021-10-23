using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace CsvProc9000.Csv
{
    public class CsvFile
    {
        private readonly List<CsvColumn> _columns = new();
        private readonly List<CsvRow> _rows = new();

        public CsvFile([NotNull] IEnumerable<string> columns)
        {
            if (columns == null) throw new ArgumentNullException(nameof(columns));

            var columnsArray = columns.ToArray();
            for (var index = 0; index < columnsArray.Length; index++)
            {
                _columns.Add(new CsvColumn(index, columnsArray[index]));
            }
        }

        public IReadOnlyCollection<CsvColumn> Columns => _columns;

        public IReadOnlyCollection<CsvRow> Rows => _rows;

        public void AddRow([NotNull] CsvRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            
            _rows.Add(row);
        }
    }
}