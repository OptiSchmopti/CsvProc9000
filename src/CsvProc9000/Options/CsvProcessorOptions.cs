using JetBrains.Annotations;

namespace CsvProc9000.Options
{
    public class CsvProcessorOptions
    {
        [UsedImplicitly]
        public string Inbox { get; set; }
        
        [UsedImplicitly]
        public string Outbox { get; set; }
    }
}