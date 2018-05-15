using System;
using Xer.DomainDriven;

namespace Domain.DomainEvents
{
    public class ProductActivatedEvent : IDomainEvent
    {
        public Guid AggregateRootId { get; }

        public DateTime TimeStamp { get; } = DateTime.UtcNow;

        public ProductActivatedEvent(Guid productId)
        {
            AggregateRootId = productId;
        }
    }
}