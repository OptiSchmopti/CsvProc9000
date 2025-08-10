using JetBrains.Annotations;
using System.Collections.Generic;

namespace CsvProc9000.Model.Configuration;

public class CsvProcessorOptions
{
    [UsedImplicitly]
    public int JobThreadCount { get; set; } = 2;

    [UsedImplicitly]
    public string Inbox { get; set; }

    [UsedImplicitly]
    public string InboxDelimiter { get; set; }

    [UsedImplicitly]
    public bool DeleteInboxFile { get; set; } = true;

    [UsedImplicitly]
    public List<OutboxRule> OutboxRules { get; set; } = [];

    [UsedImplicitly]
    public List<Rule> Rules { get; set; }
}
