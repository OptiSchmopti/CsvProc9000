using JetBrains.Annotations;

namespace CsvProc9000.Model.Configuration
{
    public class Condition
    {
        [UsedImplicitly]
        public string Field { get; set; }

        [UsedImplicitly]
        public string Value { get; set; }
    }
}
