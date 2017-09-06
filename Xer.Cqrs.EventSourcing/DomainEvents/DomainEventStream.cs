using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xer.Cqrs.EventSourcing.Exceptions;

namespace Xer.Cqrs.EventSourcing.DomainEvents
{
    public class DomainEventStream : IEnumerable<IDomainEvent>
    {
        /// <summary>
        /// Empty stream.
        /// </summary>
        public static readonly DomainEventStream Empty = new DomainEventStream(Guid.Empty, Enumerable.Empty<IDomainEvent>());

        private readonly Queue<IDomainEvent> _domainEvents;

        /// <summary>
        /// Id of the aggregate which owns this stream.
        /// </summary>
        public Guid AggregateId { get; }

        /// <summary>
        /// Version of the latest domain event in this stream.
        /// </summary>
        public int LatestDomainEventVersion { get; }

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

            List<IDomainEvent> copy = new List<IDomainEvent>(domainEvents);

            if (copy.Count > 0)
            {
                LatestDomainEventVersion = copy[copy.Count - 1].Version;
            }

            _domainEvents = new Queue<IDomainEvent>(copy);
        }

        /// <summary>
        /// Check if this stream's domain events have a greater version than the other's oldest domain event.
        /// </summary>
        /// <param name="other">Other domain event stream.</param>
        /// <returns>True, if latest version in this stream has a greater version than the other's oldest domain event. Otherwise, false.</returns>
        public bool HasGreaterVersionThan(DomainEventStream other)
        {
            IDomainEvent firstOtherDomainEvent = other._domainEvents.FirstOrDefault();
            if(firstOtherDomainEvent == null)
            {
                return true;
            }

            return LatestDomainEventVersion > firstOtherDomainEvent.Version;
        }

        public DomainEventStream AppendDomainEvent(IDomainEvent domainEventToAppend)
        {
            if (LatestDomainEventVersion >= domainEventToAppend.Version)
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

        public DomainEventStream AppendStream(DomainEventStream streamToAppend)
        {
            if (HasGreaterVersionThan(streamToAppend))
            {
                throw new DomainEventStreamVersionConflictException(streamToAppend,
                    "Domain event stream being appended contains some entries that are older than the latest event in the stream.");
            }

            if (AggregateId != streamToAppend.AggregateId)
            {
                throw new InvalidOperationException("Cannot append streams of different aggregates.");
            }

            List<IDomainEvent> mergedStream = new List<IDomainEvent>(this);
            mergedStream.AddRange(streamToAppend);

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
