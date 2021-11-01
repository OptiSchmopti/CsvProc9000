using JetBrains.Annotations;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CsvProc9000.Model.Configuration
{
    [ExcludeFromCodeCoverage] // DTO
    public class Rule
    {
        [UsedImplicitly]
        public List<Condition> Conditions { get; set; }

        [UsedImplicitly]
        public List<Change> Changes { get; set; }
    }
}
