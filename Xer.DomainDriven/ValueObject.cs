using System;

namespace Xer.DomainDriven
{
    public abstract class ValueObject<T> : IEquatable<T> where T : class
    {
        protected abstract bool ValueEquals(T other);

        public override bool Equals(object obj)
        {
            T other = obj as T;
            if(other == null)
            {
                return false;
            }

            return ValueEquals(other);
        }

        public bool Equals(T other)
        {
            return ValueEquals(other);
        }

        public static bool operator ==(ValueObject<T> obj1, ValueObject<T> obj2)
        {
            if(ReferenceEquals(obj1, null) && ReferenceEquals(obj2, null))
            {
                return true;
            }

            if (!ReferenceEquals(obj1, null) && !ReferenceEquals(obj2, null))
            {
                return obj1.Equals(obj2);
            }

            return false;
        }

        public static bool operator !=(ValueObject<T> obj1, ValueObject<T> obj2)
        {
            return !(obj1 == obj2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
