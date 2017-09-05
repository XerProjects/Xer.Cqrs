using System;

namespace Xer.Cqrs
{
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        protected abstract bool ValueEquals(ValueObject other);

        public override bool Equals(object obj)
        {
            ValueObject other = obj as ValueObject;
            if(other == null)
            {
                return false;
            }

            return ValueEquals(other);
        }

        public bool Equals(ValueObject other)
        {
            return ValueEquals(other);
        }

        public static bool operator ==(ValueObject obj1, ValueObject obj2)
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

        public static bool operator !=(ValueObject obj1, ValueObject obj2)
        {
            return !(obj1 == obj2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
