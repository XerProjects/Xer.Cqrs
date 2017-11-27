using System;

namespace Xer.DomainDriven
{
    public abstract class Entity<TId> : IEntity<TId> where TId : IEquatable<TId>
    {
        /// <summary>
        /// Unique identifier.
        /// </summary>
        public TId Id { get; protected set; }

        /// <summary>
        /// Date when object was created.
        /// </summary>
        public DateTime Created { get; protected set; }

        /// <summary>
        /// Date when object3 was last updated.
        /// </summary>
        public DateTime Updated { get; protected set; }

        public Entity(TId entityId)
        {
            Id = entityId;
            Created = DateTime.Now;
            Updated = DateTime.Now;
        }

        public virtual bool Equals(IEntity<TId> other)
        {
            if (other == null)
            {
                return false;
            }

            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            Entity<TId> entity = obj as Entity<TId>;
            if (entity == null)
            {
                return false;
            }

            return Equals(entity);
        }

        public static bool operator ==(Entity<TId> obj1, Entity<TId> obj2)
        {
            if (ReferenceEquals(obj1, null) && ReferenceEquals(obj2, null))
            {
                return true;
            }

            if (!ReferenceEquals(obj1, null) && !ReferenceEquals(obj2, null))
            {
                return obj1.Equals(obj2);
            }

            return false;
        }

        public static bool operator !=(Entity<TId> obj1, Entity<TId> obj2)
        {
            return !(obj1 == obj2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
