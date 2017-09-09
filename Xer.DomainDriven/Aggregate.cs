using System;
using System.Collections.Generic;

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
