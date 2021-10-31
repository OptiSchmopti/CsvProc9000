namespace CsvProc9000.Model
{
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
