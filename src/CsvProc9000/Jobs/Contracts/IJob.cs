using System;

namespace CsvProc9000.Jobs.Contracts;

internal interface IJob
{
    Guid Id { get; }

    DateTimeOffset Creation { get; }

    DateTimeOffset ExecutionStart { get; }

    DateTimeOffset ExecutionFinish { get; }

    void IndicateExecutionStart();

    void IndicateExecutionFinish();
}