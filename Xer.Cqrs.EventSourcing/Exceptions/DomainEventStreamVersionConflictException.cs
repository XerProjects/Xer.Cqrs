using System;
using Xer.Cqrs.EventSourcing.DomainEvents;

namespace Xer.Cqrs.EventSourcing.Exceptions
{
    public class DomainEventStreamVersionConflictException : Exception
    {
        public Guid AggregateId { get; }
        public DomainEventStream DomainEventStream { get; }

        public DomainEventStreamVersionConflictException(DomainEventStream domainEventStream) 
            : this(domainEventStream, string.Empty)
        {
        }

        public DomainEventStreamVersionConflictException(DomainEventStream domainEventStream, string message) 
            : this(domainEventStream, message, null)
        {
        }

        public DomainEventStreamVersionConflictException(DomainEventStream domainEventStream, string message, Exception innerException) 
            : base(message, innerException)
        {
            AggregateId = domainEventStream.AggregateId;
            DomainEventStream = domainEventStream;
        }
    }
}
