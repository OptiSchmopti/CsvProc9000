using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using System.Linq;

namespace CsvProc9000.Csv
{
    public class CsvRow
    {
        private readonly List<CsvField> _fields = new();

        public IEnumerable<CsvField> Fields => _fields;

        public void AddField([NotNull] CsvColumn column, [NotNull] string fieldValue)
        {
            if (column == null) throw new ArgumentNullException(nameof(column));
            if (fieldValue == null) throw new ArgumentNullException(nameof(fieldValue));
            
            _fields.Add(new CsvField(column, fieldValue));
        }

        public void AddField([NotNull] string fieldName, [NotNull] string fieldValue)
        {
            if (fieldValue == null) throw new ArgumentNullException(nameof(fieldValue));
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(fieldName));

            var nextIndexForColumn = Fields.Count() + 1;
            var column = new CsvColumn(nextIndexForColumn, fieldName);
            
            AddField(column, fieldValue);
        }

        public void AddOrUpdateField([NotNull] string fieldName, [NotNull] string fieldValue, int? fieldIndex)
        {
            if (fieldValue == null) throw new ArgumentNullException(nameof(fieldValue));
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(fieldName));

            if (!TryGetCandidateToChange(fieldName, fieldIndex, out var fieldToChange))
            {
                AddField(fieldName, fieldValue);
                return;
            }

            var index = _fields.IndexOf(fieldToChange);
            _fields.Remove(fieldToChange);
            
            var changedField = fieldToChange with { Value = fieldValue };
            _fields.Insert(index, changedField);
        }

        private bool TryGetCandidateToChange(string fieldName, int? fieldIndex, out CsvField fieldToChange)
        {
            // get possible fields with the given name
            var fieldCandidates = _fields
                .Where(field => field.Column.Name == fieldName)
                .ToList();

            fieldToChange = null;

            // when we found more than one field with that name, we need to find a possible field with the given index
            if (fieldCandidates.Count > 1)
            {
                if (!fieldIndex.HasValue)
                    throw new ArgumentException(
                        $"Found more than one candidate for field name {fieldName} but no field index was provided",
                        nameof(fieldIndex));

                fieldToChange = fieldCandidates.FirstOrDefault(field => field.Column.Index == fieldIndex.Value);
            }
            else
                fieldToChange = fieldCandidates.FirstOrDefault();

            return fieldToChange != null;
        }
    }
}