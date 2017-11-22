using System;

namespace Xer.DomainDriven
{
    public abstract class Entity : IEntity
    {
        /// <summary>
        /// Unique identifier.
        /// </summary>
        public Guid Id { get; protected set; }

        /// <summary>
        /// Date when object was created.
        /// </summary>
        public DateTime Created { get; protected set; }

        /// <summary>
        /// Date when object3 was last updated.
        /// </summary>
        public DateTime Updated { get; protected set; }

        public Entity(Guid entityId)
        {
            Id = entityId;
            Created = DateTime.Now;
            Updated = DateTime.Now;
        }
    }
}
