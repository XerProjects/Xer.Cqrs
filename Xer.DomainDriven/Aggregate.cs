using System;

namespace Xer.DomainDriven
{
    public abstract class Aggregate<TId> : Entity<TId>, IAggregate<TId> where TId : IEquatable<TId>
    {
        public Aggregate(TId aggregateId) 
            : base(aggregateId)
        {
        }
    }
}
