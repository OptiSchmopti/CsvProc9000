using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CsvProc9000.Csv
{
    public class CsvRow
    {
        private readonly List<CsvField> _fields = new();

        public IReadOnlyCollection<CsvField> Fields => _fields;

        public void AddField([NotNull] CsvColumn column, [NotNull] string fieldValue)
        {
            if (column == null) throw new ArgumentNullException(nameof(column));
            if (fieldValue == null) throw new ArgumentNullException(nameof(fieldValue));

            _fields.Add(new CsvField(column, fieldValue));
        }
    }
}