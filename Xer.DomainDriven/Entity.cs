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
    }
}
