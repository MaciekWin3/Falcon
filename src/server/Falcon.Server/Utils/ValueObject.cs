namespace Falcon.Server.Utils
{
    public abstract class ValueObject
    {
        protected abstract IEnumerable<object> GetEqualityComponents();

        public override bool Equals(object obj)
        {
            return Equals(obj as ValueObject);
        }

        private bool Equals(ValueObject valueObject)
        {
            if (valueObject == null)
            {
                return false;
            }

            if (GetRealType() != valueObject.GetRealType())
            {
                return false;
            }

            return GetEqualityComponents().SequenceEqual(valueObject.GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Aggregate(1, (current, obj) =>
                {
                    unchecked
                    {
                        return current * 23 + (obj?.GetHashCode() ?? 0);
                    }
                });
        }

        private Type GetRealType()
        {
            var type = GetType();

            return type.ToString().Contains("Castle.Proxies.", StringComparison.Ordinal) ? type.BaseType : type;
        }
    }
}