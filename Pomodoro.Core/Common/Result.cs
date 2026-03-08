namespace Pomodoro.Core.Common
{
    public readonly record struct Error(string Code, string Message)
    {
        public static readonly Error None = new(string.Empty, string.Empty);
    }

    public sealed class Result
    {
        private Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None)
            {
                throw new ArgumentException("A successful result cannot contain an error.");
            }

            if (!isSuccess && error == Error.None)
            {
                throw new ArgumentException("A failure result must contain an error.");
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public Error Error { get; }

        public static Result Success() => new(true, Error.None);

        public static Result Failure(Error error) => new(false, error);
    }

    public sealed class Result<TValue>
    {
        private readonly TValue? _value;

        private Result(TValue value)
        {
            IsSuccess = true;
            _value = value;
            Error = Error.None;
        }

        private Result(Error error)
        {
            if (error == Error.None)
            {
                throw new ArgumentException("A failure result must contain an error.");
            }

            IsSuccess = false;
            _value = default;
            Error = error;
        }

        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public Error Error { get; }

        public TValue Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access Value on a failed result.");

        public static Result<TValue> Success(TValue value) => new(value);

        public static Result<TValue> Failure(Error error) => new(error);
    }
}
