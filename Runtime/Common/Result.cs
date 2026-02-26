using System;

namespace YagizEraslan.DeepSeek.Unity
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public T Value { get; }
        public string Error { get; }
        public Exception Exception { get; }
        public int? HttpStatusCode { get; }

        private Result(T value, bool isSuccess, string error, Exception exception = null, int? httpStatusCode = null)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
            Exception = exception;
            HttpStatusCode = httpStatusCode;
        }

        public static Result<T> Success(T value) => new Result<T>(value, true, null);

        public static Result<T> Failure(string error, Exception exception = null, int? httpStatusCode = null)
            => new Result<T>(default, false, error, exception, httpStatusCode);

        public void Match(Action<T> onSuccess, Action<string> onFailure)
        {
            if (IsSuccess) onSuccess?.Invoke(Value);
            else onFailure?.Invoke(Error);
        }

        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        {
            return IsSuccess ? onSuccess(Value) : onFailure(Error);
        }

        public T GetValueOrDefault(T defaultValue = default) => IsSuccess ? Value : defaultValue;
    }
}
