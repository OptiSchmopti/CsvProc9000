using JetBrains.Annotations;
using System.Collections.Generic;

namespace CsvProc9000.Model.Configuration
{
    public class Rule
    {
        [UsedImplicitly]
        public List<Condition> Conditions { get; set; }

        [UsedImplicitly]
        public List<Change> Changes { get; set; }
    }
}
