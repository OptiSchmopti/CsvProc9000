using JetBrains.Annotations;
using System.Diagnostics.CodeAnalysis;

namespace CsvProc9000.Model.Configuration
{
    [ExcludeFromCodeCoverage] // DTO
    public class Condition
    {
        [UsedImplicitly]
        public string Field { get; set; }

        [UsedImplicitly]
        public string Value { get; set; }
    }
}
