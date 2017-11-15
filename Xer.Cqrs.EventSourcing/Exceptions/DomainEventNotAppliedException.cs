using System;

namespace Xer.Cqrs.EventSourcing.Exceptions
{
    public class DomainEventNotAppliedException : Exception
    {
        public Guid AggregateId { get; }
        public IDomainEvent DomainEvent { get; }

        public DomainEventNotAppliedException(IDomainEvent domainEvent) 
            : this(domainEvent, string.Empty)
        {
        }

        public DomainEventNotAppliedException(IDomainEvent domainEvent, string message) 
            : this(domainEvent, message, null)
        {
        }

        public DomainEventNotAppliedException(IDomainEvent domainEvent, string message, Exception innerException) 
            : base(message, innerException)
        {
            AggregateId = domainEvent.AggregateId;
            DomainEvent = domainEvent;
        }
    }
}
