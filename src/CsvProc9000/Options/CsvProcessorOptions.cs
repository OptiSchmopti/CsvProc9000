using System.Collections.Generic;
using JetBrains.Annotations;

namespace CsvProc9000.Options
{
    public class CsvProcessorOptions
    {
        [UsedImplicitly]
        public string Inbox { get; set; }
        
        [UsedImplicitly]
        public string InboxDelimiter { get; set; }
        
        [UsedImplicitly]
        public string Outbox { get; set; }
        
        [UsedImplicitly]
        public string OutboxDelimiter { get; set; }
        
        [UsedImplicitly]
        public List<Rule> Rules { get; set; }
    }
}