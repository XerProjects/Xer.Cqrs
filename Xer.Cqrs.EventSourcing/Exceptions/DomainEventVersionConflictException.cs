using System;

namespace Xer.Cqrs.EventSourcing.Exceptions
{
    public class DomainEventVersionConflictException : Exception
    {
        public Guid AggregateId { get; }
        public DomainEventStream DomainEventStream { get; }

        public DomainEventVersionConflictException(DomainEventStream domainEventStream, string message, Exception innerException) 
            : base(message, innerException)
        {
            AggregateId = domainEventStream.AggregateId;
            DomainEventStream = domainEventStream;
        }

        public DomainEventVersionConflictException(DomainEventStream domainEventStream, string message) 
            : this(domainEventStream, message, null)
        {
        }

        public DomainEventVersionConflictException(DomainEventStream domainEventStream) 
            : this(domainEventStream, string.Empty)
        {
        }

        public DomainEventVersionConflictException(IDomainEvent domainEvent, string message, Exception innerException) 
            : this(new DomainEventStream(domainEvent.AggregateId, new[] { domainEvent }), message, innerException)
        {
        }

        public DomainEventVersionConflictException(IDomainEvent domainEvent, string message) 
            : this(domainEvent, message, null)
        {
        }

        public DomainEventVersionConflictException(IDomainEvent domainEvent) 
            : this(domainEvent, string.Empty)
        {
        }
    }
}
