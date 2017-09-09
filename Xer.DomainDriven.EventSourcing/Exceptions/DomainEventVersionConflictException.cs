using System;
using Xer.DomainDriven.EventSourcing.DomainEvents;

namespace Xer.DomainDriven.EventSourcing.Exceptions
{
    public class DomainEventVersionConflictException : Exception
    {
        public Guid AggregateId { get; }
        public IDomainEvent DomainEvent { get; }

        public DomainEventVersionConflictException(IDomainEvent domainEvent)
            : this(domainEvent, string.Empty)
        {
        }

        public DomainEventVersionConflictException(IDomainEvent domainEvent, string message)
            : this(domainEvent, message, null)
        {
        }

        public DomainEventVersionConflictException(IDomainEvent domainEvent, string message, Exception innerException)
            : base(message, innerException)
        {
            AggregateId = domainEvent.AggregateId;
            DomainEvent = domainEvent;
        }
    }
}
