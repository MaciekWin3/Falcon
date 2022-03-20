using Falcon.Server.Errors;

namespace Falcon.Server.Utils
{
    public class Result
    {
        public bool IsSuccess { get; }
        public Error Error { get; }
        public bool IsFailure => !IsSuccess;

        protected Result(bool isSuccess, Error error)
        {
            if (error == null)
            {
                throw new InvalidOperationException();
            }
            if (isSuccess && !string.IsNullOrEmpty(error.Message))
            {
                throw new InvalidOperationException();
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Fail(Error error)
        {
            return new Result(false, error);
        }

        public static Result<T> Fail<T>(Error error)
        {
            return new Result<T>(default, false, error);
        }

        public static Result Ok()
        {
            return new Result(true, new Error(string.Empty, string.Empty));
        }

        public static Result<T> Ok<T>(T value)
        {
            return new Result<T>(value, true, new Error(string.Empty, string.Empty));
        }

        public static implicit operator Result(Error error) => error.ToResult();
    }
}