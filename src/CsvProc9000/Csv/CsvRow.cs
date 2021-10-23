using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CsvProc9000.Csv
{
    public class CsvRow
    {
        private readonly List<CsvField> _fields = new();

        public IEnumerable<CsvField> Fields => _fields;

        public void AddOrUpdateField([NotNull] string fieldName, [NotNull] string fieldValue)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(fieldName));
            if (fieldValue == null) throw new ArgumentNullException(nameof(fieldValue));

            _fields.Add(new CsvField(fieldName, fieldValue));
        }
    }
}