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
    public string Outbox { get; set; }

    [UsedImplicitly]
    public string OutboxDelimiter { get; set; }

    [UsedImplicitly]
    public bool OutboxValuesInQuotes { get; set; }

    [UsedImplicitly]
    public string OutboxFileCharset { get; set; } = "UTF-8";

    [UsedImplicitly]
    public List<Rule> Rules { get; set; }
}
