using JetBrains.Annotations;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CsvProc9000.Model.Configuration;

[ExcludeFromCodeCoverage] // DTO
public class OutboxRule
{
    [UsedImplicitly]
    public string Name { get; set; }

    [UsedImplicitly]
    public List<Condition> Conditions { get; set; }

    [UsedImplicitly]
    public string Outbox { get; set; }

    [UsedImplicitly]
    public string OutboxDelimiter { get; set; }

    [UsedImplicitly]
    public bool OutboxValuesInQuotes { get; set; }

    [UsedImplicitly]
    public string OutboxFileCharset { get; set; } = "UTF-8";

    public string GetName()
    {
        return string.IsNullOrWhiteSpace(Name)
            ? "< Unknown >"
            : Name;
    }
}
