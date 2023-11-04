using JetBrains.Annotations;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CsvProc9000.Model.Configuration
{
    [ExcludeFromCodeCoverage] // DTO
    public class Rule
    {
        [UsedImplicitly]
        public string Name { get; set; }
        
        [UsedImplicitly]
        public List<Condition> Conditions { get; set; }

        [UsedImplicitly]
        public List<Change> Changes { get; set; }

        public string GetName()
        {
            return string.IsNullOrWhiteSpace(Name) 
                ? "< Unknown >" 
                : Name;
        }
    }
}
