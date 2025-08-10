using CsvProc9000.Csv.Contracts;
using CsvProc9000.Model.Configuration;
using CsvProc9000.Model.Csv;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace CsvProc9000.Csv;

public class ApplyRulesToCsvFile : IApplyRulesToCsvFile
{
    private readonly ILogger<ApplyRulesToCsvFile> _logger;
    private readonly CsvProcessorOptions _processorOptions;

    public ApplyRulesToCsvFile(
        [NotNull] IOptions<CsvProcessorOptions> processorOptions,
        [NotNull] ILogger<ApplyRulesToCsvFile> logger)
    {
        _processorOptions = processorOptions.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Apply(
        [NotNull] CsvFile csvFile,
        Guid jobId,
        Guid jobThreadId)
    {
        if (csvFile == null) throw new ArgumentNullException(nameof(csvFile));

        _logger.LogDebug("T-{ThreadId} J-{JobId}# Applying rules to file {File}...",
            jobThreadId, jobId, csvFile.OriginalFileName);

        if (_processorOptions.Rules == null || !_processorOptions.Rules.Any())
        {
            _logger.LogWarning(
                "T-{ThreadId} J-{JobId}# Cannot process file {File} because there are no rules defined!",
                jobThreadId, jobId, csvFile.OriginalFileName);
            return;
        }

        foreach (var rule in _processorOptions.Rules)
            ApplyRuleToFile(csvFile, rule, jobId, jobThreadId);
    }

    private void ApplyRuleToFile(
        CsvFile csvFile,
        FieldRule fieldRule,
        Guid jobId,
        Guid jobThreadId)
    {
        if (fieldRule.Conditions == null || !fieldRule.Conditions.Any())
        {
            _logger.LogWarning(
                "T-{ThreadId} J-{JobId}# Skipping rule '{RuleName}' because it has no conditions!",
                jobThreadId, jobId, fieldRule.Name);

            return;
        }

        foreach (var row in csvFile.Rows)
            ApplyRuleToRow(row, fieldRule, csvFile, jobId, jobThreadId);
    }

    private void ApplyRuleToRow(
        CsvRow row,
        FieldRule fieldRule,
        CsvFile file,
        Guid jobId,
        Guid jobThreadId)
    {
        if (!row.MeetsConditions(fieldRule.Conditions))
        {
            _logger.LogTrace(
                "T-{ThreadId} J-{JobId}# Row at Index {RowIndex} does not meet conditions of rule '{RuleName}'",
                jobThreadId, jobId, IndexOfRow(row, file), fieldRule.Name);
            return;
        }

        _logger.LogTrace(
            "T-{ThreadId} J-{JobId}# Row at index {RowIndex} meets rule '{RuleName}'. Applying change(s)...",
            jobThreadId, jobId, IndexOfRow(row, file), fieldRule.Name);

        foreach (var change in fieldRule.Changes)
            try
            {
                ApplyChangeToRow(row, fieldRule, file, change, jobId, jobThreadId);
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "T-{ThreadId} J-{JobId}# Error occured while applying change at index {ChangeIndex} to row at index {RowIndex}",
                    jobThreadId, jobId, IndexOfChange(fieldRule, change), IndexOfRow(row, file));
            }
    }

    private void ApplyChangeToRow(
        CsvRow row,
        FieldRule fieldRule,
        CsvFile file,
        Change change,
        Guid jobId,
        Guid jobThreadId)
    {
        if (string.IsNullOrWhiteSpace(change.Field))
        {
            _logger.LogWarning(
                "T-{ThreadId} J-{JobId}# Not applying change at index {ChangeIndex} for rule '{RuleName}' because no field name given",
                jobThreadId, jobId, IndexOfChange(fieldRule, change), fieldRule.Name);

            return;
        }

        _logger.LogTrace(
            "T-{ThreadId} J-{JobId}# Row at index {RowIndex}: Applying change at index {ChangeIndex}: Field={Field}, Value={Value}, Mode={Mode}, Index={Index}",
            jobThreadId, jobId, IndexOfRow(row, file), IndexOfChange(fieldRule, change), change.Field,
            change.Value, change.Mode, change.FieldIndex);

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

    private static int IndexOfRow(CsvRow row, CsvFile file)
    {
        return file.Rows.ToList().IndexOf(row);
    }

    private static int IndexOfChange(FieldRule fieldRule, Change change)
    {
        return fieldRule.Changes.IndexOf(change);
    }
}
