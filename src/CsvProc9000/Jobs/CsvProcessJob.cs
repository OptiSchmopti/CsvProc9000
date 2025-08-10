using CsvProc9000.Jobs.Contracts;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;

namespace CsvProc9000.Jobs;

[ExcludeFromCodeCoverage] // DTO
internal sealed class CsvProcessJob : IJob
{
    public CsvProcessJob([NotNull] IFileInfo targetFile)
    {
        TargetFile = targetFile ?? throw new ArgumentNullException(nameof(targetFile));

        Id = Guid.NewGuid();
        Creation = DateTimeOffset.Now;
    }

    public IFileInfo TargetFile { get; }

    public Guid Id { get; }

    public DateTimeOffset Creation { get; }

    public DateTimeOffset ExecutionStart { get; private set; }

    public DateTimeOffset ExecutionFinish { get; private set; }

    public void IndicateExecutionStart()
    {
        ExecutionStart = DateTimeOffset.Now;
    }

    public void IndicateExecutionFinish()
    {
        ExecutionFinish = DateTimeOffset.Now;
    }
}