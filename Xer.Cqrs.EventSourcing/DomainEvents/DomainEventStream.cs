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
        /// Version of the first domain event in this stream.
        /// </summary>
        public int FirstDomainEventVersion { get; }

        /// <summary>
        /// Version of the latest domain event in this stream.
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
            DomainEventCount = domainEvents.Count();
            
            if (DomainEventCount > 0)
            {
                FirstDomainEventVersion = domainEvents.First().AggregateVersion;
                LastDomainEventVersion = domainEvents.Last().AggregateVersion;
            }

            _domainEvents = new List<IDomainEvent>(domainEvents);
        }

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
