using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xer.Cqrs.EventSourcing.Exceptions;

namespace Xer.Cqrs.EventSourcing.DomainEvents
{
    public sealed class DomainEventStream : IEnumerable<IDomainEvent>
    {
        /// <summary>
        /// Empty stream.
        /// </summary>
        public static readonly DomainEventStream Empty = new DomainEventStream(Guid.Empty, Enumerable.Empty<IDomainEvent>());

        private readonly ICollection<IDomainEvent> _domainEvents;

        /// <summary>
        /// Id of the aggregate which owns this stream.
        /// </summary>
        public Guid AggregateId { get; }
        
        /// <summary>
        /// Aggregate version of the first domain event in this stream.
        /// </summary>
        public int FirstDomainEventVersion { get; }

        /// <summary>
        /// Aggregate version of the latest domain event in this stream.
        /// </summary>
        public int LastDomainEventVersion { get; }

        /// <summary>
        /// Get number of domain events in the stream.
        /// </summary>
        public int DomainEventCount { get; }

        /// <summary>
        /// Constructs a new instance of a read-only stream.
        /// </summary>
        /// <param name="aggregateId">Id of the aggregate which owns this stream.</param>
        /// <param name="domainEvents">Domain events.</param>
        public DomainEventStream(Guid aggregateId, IEnumerable<IDomainEvent> domainEvents)
        {
            if(domainEvents == null)
            {
                throw new ArgumentNullException(nameof(domainEvents));
            }

            AggregateId = aggregateId;

            _domainEvents = new List<IDomainEvent>(domainEvents);

            DomainEventCount = _domainEvents.Count;
            
            if (DomainEventCount > 0)
            {
                FirstDomainEventVersion = domainEvents.First().AggregateVersion;
                LastDomainEventVersion = domainEvents.Last().AggregateVersion;
            }

        }

        /// <summary>
        /// Creates a new domain event stream which has the appended domain event.
        /// </summary>
        /// <param name="domainEventToAppend">Domain event to append to the domain event stream.</param>
        /// <returns>New instance of domain event stream with the appended domain event.</returns>
        public DomainEventStream AppendDomainEvent(IDomainEvent domainEventToAppend)
        {
            if (domainEventToAppend == null)
            {
                throw new ArgumentNullException(nameof(domainEventToAppend));
            }

            if (LastDomainEventVersion >= domainEventToAppend.AggregateVersion)
            {
                throw new DomainEventVersionConflictException(domainEventToAppend,
                    "Domain event being appended is older than the latest event in the stream.");
            }

            if (AggregateId != domainEventToAppend.AggregateId)
            {
                throw new InvalidOperationException("Cannot append domain event of different aggregate.");
            }

            List<IDomainEvent> mergedStream = new List<IDomainEvent>(this);
            mergedStream.Add(domainEventToAppend);

            return new DomainEventStream(AggregateId, mergedStream);
        }

        /// <summary>
        /// Creates a new domain event stream which has the appended domain event stream.
        /// </summary>
        /// <param name="streamToAppend">Domain event stream to append to this domain event stream.</param>
        /// <returns>New instance of domain event stream with the appended domain event stream.</returns>
        public DomainEventStream AppendDomainEventStream(DomainEventStream streamToAppend)
        {
            if(streamToAppend == null)
            {
                throw new ArgumentNullException(nameof(streamToAppend));
            }

            if (LastDomainEventVersion >= streamToAppend.FirstDomainEventVersion)
            {
                throw new DomainEventStreamVersionConflictException(streamToAppend,
                    "Domain event stream being appended contains some entries that are older than the latest event in the stream.");
            }

            if (AggregateId != streamToAppend.AggregateId)
            {
                throw new InvalidOperationException("Cannot append streams of different aggregates.");
            }

            IEnumerable<IDomainEvent> mergedStream = this.Concat(streamToAppend);

            return new DomainEventStream(AggregateId, streamToAppend);
        }

        /// <summary>
        /// Get enumerator.
        /// </summary>
        /// <returns>Enumerator.</returns>
        public IEnumerator<IDomainEvent> GetEnumerator()
        {
            return _domainEvents.GetEnumerator();
        }

        /// <summary>
        /// Get enumerator.
        /// </summary>
        /// <returns>Enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _domainEvents.GetEnumerator();
        }
    }
}
