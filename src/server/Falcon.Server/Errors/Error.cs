using Falcon.Server.Utils;

namespace Falcon.Server.Errors
{
    public sealed class Error : ValueObject
    {
        public string Code { get; }
        public string Message { get; }
        public string Details { get; }

        public Error(string code, string message, string details = "")
        {
            Code = code;
            Message = message;
            Details = details;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Code;
        }
    }
}