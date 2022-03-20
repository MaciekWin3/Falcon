using Falcon.Server.Errors;

namespace Falcon.Server.Utils
{
    public class Result<T> : Result
    {
        private readonly T value;

        public T Value
        {
            get
            {
                if (!IsSuccess)
                {
                    throw new InvalidOperationException();
                }

                return value;
            }
        }

        public bool TryGetValue(out T result)
        {
            if (IsSuccess)
            {
                result = Value;
                return true;
            }

            result = default;
            return false;
        }

        protected internal Result(T value, bool isSuccess, Error error = null) : base(isSuccess, error)
        {
            this.value = value;
        }

        public static implicit operator Result<T>(Error error) => error.ToResult<T>();

        public static implicit operator Result<T>(T value) => Ok(value);
    }
}