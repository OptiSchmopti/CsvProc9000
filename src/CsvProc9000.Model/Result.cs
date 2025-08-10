using System.Diagnostics.CodeAnalysis;

namespace CsvProc9000.Model;

[ExcludeFromCodeCoverage] // DTO
public class Result<T>
{
    public Result(
        bool isSuccess,
        T value = default,
        string failureMessage = default)
    {
        IsSuccess = isSuccess;
        Value = value;
        FailureMessage = failureMessage ?? string.Empty;
    }

    public bool IsSuccess { get; }

    public T Value { get; }

    public string FailureMessage { get; }
}