using System.Diagnostics.CodeAnalysis;

namespace CsvProc9000.Model
{
    [ExcludeFromCodeCoverage] // DTO
    public class Result<T>
    {
        public Result(
            bool isSuccess,
            T value = default)
        {
            IsSuccess = isSuccess;
            Value = value;
        }

        public bool IsSuccess { get; }

        public T Value { get; }
    }
}
