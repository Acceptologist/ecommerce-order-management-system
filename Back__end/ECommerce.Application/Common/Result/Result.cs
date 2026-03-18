namespace ECommerce.Application.Common.Result
{
    /// <summary>
    /// Simple generic wrapper used to standardize handler results.  Future
    /// handlers can return <see cref="Result{T}"/> instead of raw values or
    /// throwing exceptions, making success/failure outcomes explicit.
    /// </summary>
    public class Result<T>
    {
        private Result(bool isSuccess, T? value, string? error)
        {
            IsSuccess = isSuccess;
            Value = value!;
            Error = error;
        }

        /// <summary>
        /// Indicates whether the operation succeeded.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Error message when <see cref="IsSuccess"/> is <c>false</c>.
        /// </summary>
        public string? Error { get; }

        /// <summary>
        /// Value produced when <see cref="IsSuccess"/> is <c>true</c>.
        /// </summary>
        public T Value { get; }

        public static Result<T> Success(T value)
        {
            return new Result<T>(true, value, null);
        }

        public static Result<T> Failure(string error)
        {
            return new Result<T>(false, default!, error);
        }
    }
}