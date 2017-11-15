using System;

namespace Xer.DomainDriven
{
    public abstract class Aggregate : Entity, IAggregate
    {
        public Aggregate(Guid aggregateId) 
            : base(aggregateId)
        {
        }
    }
}
