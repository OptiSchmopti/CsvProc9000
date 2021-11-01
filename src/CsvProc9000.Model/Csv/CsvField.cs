using System.Diagnostics.CodeAnalysis;

namespace CsvProc9000.Model.Csv
{
    [ExcludeFromCodeCoverage] // DTO
    public record CsvField(CsvColumn Column, string Value);
}
