using System;
using Xer.DomainDriven;

namespace Domain.DomainEvents
{
    public class ProductDeactivatedEvent : IDomainEvent
    {
        public Guid AggregateRootId { get; }

        public DateTime TimeStamp { get; } = DateTime.UtcNow;


        public ProductDeactivatedEvent(Guid productId)
        {
            AggregateRootId = productId;
        }
    }
}