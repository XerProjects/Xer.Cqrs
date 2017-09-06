using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Xer.Cqrs.EventSourcing.DomainEvents;

namespace Xer.Cqrs.EventSourcing.Exceptions
{
    public class UnableToApplyDomainEventException : Exception
    {
        public Guid AggregateId { get; }
        public IDomainEvent DomainEvent { get; }

        public UnableToApplyDomainEventException(IDomainEvent domainEvent) 
            : this(domainEvent, string.Empty)
        {
        }

        public UnableToApplyDomainEventException(IDomainEvent domainEvent, string message) 
            : this(domainEvent, message, null)
        {
        }

        public UnableToApplyDomainEventException(IDomainEvent domainEvent, string message, Exception innerException) 
            : base(message, innerException)
        {
            AggregateId = domainEvent.AggregateId;
            DomainEvent = domainEvent;
        }
    }
}
