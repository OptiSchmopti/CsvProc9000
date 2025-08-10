using CsvProc9000.Model.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CsvProc9000.Model.Csv;

// TODO: Move this logic into a service of some kind?
[ExcludeFromCodeCoverage] // DTO - yes there's some logic here, but it's mostly tested through other tests
public class CsvRow
{
    private readonly List<CsvField> _fields = new();

    public IEnumerable<CsvField> Fields => _fields;

    public void AddField([JetBrains.Annotations.NotNull] CsvColumn column,
        [JetBrains.Annotations.NotNull] string fieldValue)
    {
        if (column == null) throw new ArgumentNullException(nameof(column));
        if (fieldValue == null) throw new ArgumentNullException(nameof(fieldValue));

        _fields.Add(new CsvField(column, fieldValue));
    }

    public void AddField([JetBrains.Annotations.NotNull] string fieldName,
        [JetBrains.Annotations.NotNull] string fieldValue)
    {
        if (fieldValue == null) throw new ArgumentNullException(nameof(fieldValue));
        if (string.IsNullOrWhiteSpace(fieldName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(fieldName));

        var nextIndexForColumn = Fields.Count() + 1;
        var column = new CsvColumn(nextIndexForColumn, fieldName);

        AddField(column, fieldValue);
    }

    public void AddOrUpdateField([JetBrains.Annotations.NotNull] string fieldName,
        [JetBrains.Annotations.NotNull] string fieldValue, int? fieldIndex)
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

    public bool MeetsConditions(IEnumerable<Condition> conditions)
    {
        ArgumentNullException.ThrowIfNull(conditions);

        var meetsConditions = true;

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var condition in conditions)
        {
            var potentialFields =
                Fields
                // first we select every field with the desired
                .Where(field => field.Column.Name == condition.Field)
                // then we check if those fields have the desired value
                .Where(field => AppliesCondition(field.Value, condition.Value, condition.CheckMode));

            var anyFieldMatchesCondition = potentialFields.Any();

            /*
             * the conditions are met, when we found any fields that match the conditions in that row
             *
             * for clarity, what that boolean operation down there does:
             *
             * - case: meetsConditions = true, anyFieldMatchesCondition = true
             *   conditions were met, because at least on field meets the conditions in this row
             *   --> meetsConditions = true
             *
             * - case: meetsConditions = true, anyFieldMatchesCondition = false
             *   conditions are not met, because no field meets the conditions in this row
             *   --> meetsConditions = false
             *
             * - case: meetsConditions = false, anyFieldMatchesCondition = true
             *   conditions were not met before, but we need every condition to be met (AND link)
             *   --> meetsConditions = false
             *
             * - case: meetsConditions = false, anyFieldMatchesCondition = false
             *   nothing to explain here I guess
             *   --> meetsConditions = false
             */
            meetsConditions = meetsConditions && anyFieldMatchesCondition;
        }

        return meetsConditions;
    }

    private static bool AppliesCondition(string fieldValue, string conditionValue, CheckMode checkMode)
    {
        return checkMode switch
        {
            CheckMode.IsEqualTo => fieldValue == conditionValue,
            CheckMode.StartsWith => fieldValue.StartsWith(conditionValue),
            CheckMode.EndsWith => fieldValue.EndsWith(conditionValue),
            CheckMode.Contains => fieldValue.Contains(conditionValue),
            _ => throw new ArgumentOutOfRangeException(nameof(checkMode), checkMode, null),
        };
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
