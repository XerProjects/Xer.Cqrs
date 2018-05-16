using System;
using Xer.Cqrs.EventStack;
using Xer.DomainDriven;

namespace Domain.DomainEvents
{
    public class ProductRegisteredEvent : IDomainEvent
    {
        public string ProductName { get; }

        public Guid AggregateRootId { get; }

        public DateTime TimeStamp { get; } = DateTime.UtcNow;

        public ProductRegisteredEvent(Guid productId, string productName)
        {
            AggregateRootId = productId;
            ProductName = productName;
        }
    }
}