using System.Collections.Generic;
using JetBrains.Annotations;

namespace CsvProc9000.Options
{
    public class Rule
    {
        [UsedImplicitly]
        public List<Condition> Conditions { get; set; }

        [UsedImplicitly]
        public List<Change> Changes { get; set; }
    }
}