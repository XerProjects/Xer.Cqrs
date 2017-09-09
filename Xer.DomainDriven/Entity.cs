using System;

namespace Xer.DomainDriven
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; }
        public DateTime Created { get; protected set; }
        public DateTime Updated { get; protected set; }

        public Entity(Guid entityId)
        {
            Id = entityId;
            Created = DateTime.Now;
            Updated = DateTime.Now;
        }
    }
}
