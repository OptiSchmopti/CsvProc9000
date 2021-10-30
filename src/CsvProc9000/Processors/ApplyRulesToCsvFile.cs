using System;
using System.Linq;
using CsvProc9000.Csv;
using CsvProc9000.Options;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CsvProc9000.Processors
{
    public class ApplyRulesToCsvFile : IApplyRulesToCsvFile
    {
        private readonly ILogger<ApplyRulesToCsvFile> _logger;
        private readonly CsvProcessorOptions _processorOptions;

        public ApplyRulesToCsvFile(
            [NotNull] IOptions<CsvProcessorOptions> processorOptions,
            [NotNull] ILogger<ApplyRulesToCsvFile> logger)
        {
            _processorOptions = processorOptions.Value ?? throw new ArgumentNullException(nameof(processorOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Apply(CsvFile csvFile)
        {
            _logger.LogDebug("Processor: Applying rules to file {File}...", csvFile.OriginalFileName);
            
            if (_processorOptions.Rules == null || !_processorOptions.Rules.Any())
            {
                _logger.LogWarning("Processor: Cannot process file {File} because there are no rules defined",
                    csvFile.OriginalFileName);
                return;
            }

            foreach (var rule in _processorOptions.Rules)
                ApplyRuleToFile(csvFile, rule);
        }

        private void ApplyRuleToFile(CsvFile csvFile, Rule rule)
        {
            if (rule.Conditions == null || !rule.Conditions.Any())
            {
                _logger.LogWarning("Processor: Skipping Rule at index {Index} because it has no conditions",
                    _processorOptions.Rules.IndexOf(rule));
                return;
            }

            foreach (var row in csvFile.Rows)
                ApplyRuleToRow(row, rule, csvFile);
        }

        private void ApplyRuleToRow(CsvRow row, Rule rule, CsvFile file)
        {
            if (!MeetsRowConditions(row, rule))
            {
                _logger.LogTrace("Processor: Row at Index {RowIndex} does not meet conditions of rule at index {RuleIndex}",
                    file.Rows.ToList().IndexOf(row), _processorOptions.Rules.IndexOf(rule));
                return;
            }

            _logger.LogTrace("Processor: Row at index {RowIndex} meets rule at index {RuleIndex}. Applying change(s)...",
                file.Rows.ToList().IndexOf(row), _processorOptions.Rules.IndexOf(rule));

            foreach (var change in rule.Changes)
                try
                {
                    ApplyChangeToRow(row, rule, file, change);
                }
                catch (Exception e)
                {
                    _logger.LogError(e,
                        "Processor: Error occured while applying change at index {ChangeIndex} to row at index {RowIndex}",
                        rule.Changes.IndexOf(change), file.Rows.ToList().IndexOf(row));
                }
        }

        private void ApplyChangeToRow(CsvRow row, Rule rule, CsvFile file, Change change)
        {
            if (string.IsNullOrWhiteSpace(change.Field))
            {
                _logger.LogWarning(
                    "Processor: Not applying change at index {ChangeIndex} for rule at index {RuleIndex} because no field name given",
                    rule.Changes.IndexOf(change), _processorOptions.Rules.IndexOf(rule));
                return;
            }

            _logger.LogTrace(
                "Processor: Row at index {RowIndex}: Applying change at index {ChangeIndex}: Field={Field}, Value={Value}, Mode={Mode}, Index={Index}",
                file.Rows.ToList().IndexOf(row), rule.Changes.IndexOf(change), change.Field, change.Value, change.Mode, change.FieldIndex);

            switch (change.Mode)
            {
                case ChangeMode.Add:
                    row.AddField(change.Field, change.Value);
                    break;
                case ChangeMode.AddOrUpdate:
                    row.AddOrUpdateField(change.Field, change.Value, change.FieldIndex);
                    break;
                default:
#pragma warning disable CA2208
                    throw new ArgumentOutOfRangeException(nameof(change.Mode),
                        $"Unknown value {change.Mode} for {nameof(ChangeMode)}");
#pragma warning restore CA2208
            }
        }

        private static bool MeetsRowConditions(CsvRow row, Rule rule)
        {
            var meetsConditions = true;

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var condition in rule.Conditions)
            {
                var potentialFields = row
                    .Fields
                    // first we select every field with the desired
                    .Where(field => field.Column.Name == condition.Field)
                    // then we check if those fields have the desired value
                    .Where(field => field.Value == condition.Value);

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
                 *   nothing to explain here i guess
                 *   --> meetsConditions = false
                 */
                meetsConditions = meetsConditions && anyFieldMatchesCondition;
            }

            return meetsConditions;
        }
    }
}
