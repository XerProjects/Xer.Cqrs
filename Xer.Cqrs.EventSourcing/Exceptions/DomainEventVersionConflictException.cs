using System;
using System.Collections.Generic;
using System.Text;
using Xer.Cqrs.EventSourcing.DomainEvents;

namespace Xer.Cqrs.EventSourcing.Exceptions
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
