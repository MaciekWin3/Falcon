using Falcon.Server.Utils;

namespace Falcon.Server.Errors
{
    public static class ErrorExtensions
    {
        public static Result ToResult(this Error error)
        {
            return error is null
                ? Result.Ok()
                : Result.Fail(error);
        }

        public static Result<T> ToResult<T>(this Error error) => Result.Fail<T>(error);
    }
}