using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CsvProc9000.Csv
{
    public class CsvRow
    {
        private readonly List<CsvField> _fields = new();

        public IEnumerable<CsvField> Fields => _fields;

        public void AddOrUpdateField([NotNull] string columnName, [NotNull] string fieldValue)
        {
            if (columnName == null) throw new ArgumentNullException(nameof(columnName));
            if (fieldValue == null) throw new ArgumentNullException(nameof(fieldValue));

            var column = new CsvColumn(columnName);
            _fields.Add(new CsvField(column, fieldValue));
        }
    }
}