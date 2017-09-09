using System;

namespace Xer.DomainDriven
{
    public abstract class Aggregate : Entity
    {
        public Aggregate(Guid aggregateId) 
            : base(aggregateId)
        {
        }
    }
}
