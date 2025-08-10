using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CsvProc9000.Model.Csv;

[ExcludeFromCodeCoverage] // DTO
public class CsvFile
{
    private readonly List<CsvRow> _rows = new();

    public CsvFile([JetBrains.Annotations.NotNull] string fileName)
    {
        OriginalFileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
    }

    public string OriginalFileName { get; }

    public IEnumerable<CsvRow> Rows => _rows;

    public void AddRow([JetBrains.Annotations.NotNull] CsvRow row)
    {
        if (row == null) throw new ArgumentNullException(nameof(row));

        _rows.Add(row);
    }
}