using System;
using System.Collections.Generic;

namespace Xer.Cqrs
{
    public abstract class AggregateRoot : Entity
    {
        public AggregateRoot(Guid aggregateId) 
            : base(aggregateId)
        {
        }
    }
}
