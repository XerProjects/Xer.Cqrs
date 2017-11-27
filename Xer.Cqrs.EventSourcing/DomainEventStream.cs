using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xer.Cqrs.EventSourcing.Exceptions;

namespace Xer.Cqrs.EventSourcing
{
    public class DomainEventStream<TAggregateId> : IDomainEventStream<TAggregateId>, 
                                                   IEnumerable<IDomainEvent> 
                                                   where TAggregateId : IEquatable<TAggregateId>
    {
        #region Declarations
        
        private readonly ICollection<IDomainEvent> _domainEvents;

        #endregion Declarations

        #region Properties
            
        /// <summary>
        /// Id of the aggregate which owns this stream.
        /// </summary>
        public TAggregateId AggregateId { get; }
        
        /// <summary>
        /// Aggregate version of the first domain event in this stream.
        /// </summary>
        public int BeginVersion { get; }

        /// <summary>
        /// Aggregate version of the last domain event in this stream.
        /// </summary>
        public int EndVersion { get; }

        /// <summary>
        /// Get number of domain events in the stream.
        /// </summary>
        public int DomainEventCount { get; }

        #endregion Properties
        
        #region Constructors
            
        /// <summary>
        /// Constructor to create an empty stream for the aggregate.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        public DomainEventStream(TAggregateId aggreggateId)
        {
            AggregateId = aggreggateId;
            _domainEvents = new List<IDomainEvent>();
        }

        /// <summary>
        /// Constructs a new instance of a read-only stream.
        /// </summary>
        /// <param name="aggregateId">Id of the aggregate which owns this stream.</param>
        /// <param name="domainEvents">Domain events.</param>
        public DomainEventStream(TAggregateId aggregateId, IEnumerable<IDomainEvent> domainEvents)
        {
            if(domainEvents == null)
            {
                throw new ArgumentNullException(nameof(domainEvents));
            }

            _domainEvents = domainEvents.ToList();

            AggregateId = aggregateId;
            DomainEventCount = _domainEvents.Count;
            
            if (DomainEventCount > 0)
            {
                BeginVersion = domainEvents.First().AggregateVersion;
                EndVersion = domainEvents.Last().AggregateVersion;
            }
        }

        #endregion Constructors

        /// <summary>
        /// Creates a new domain event stream which has the appended domain event.
        /// </summary>
        /// <param name="domainEventToAppend">Domain event to append to the domain event stream.</param>
        /// <returns>New instance of domain event stream with the appended domain event.</returns>
        public DomainEventStream<TAggregateId> AppendDomainEvent(IDomainEvent domainEventToAppend)
        {
            if (domainEventToAppend == null)
            {
                throw new ArgumentNullException(nameof(domainEventToAppend));
            }

            return AppendDomainEventStream(new DomainEventStream<TAggregateId>(AggregateId, new[] { domainEventToAppend }));
        }

        /// <summary>
        /// Creates a new domain event stream which has the appended domain event stream.
        /// </summary>
        /// <param name="streamToAppend">Domain event stream to append to this domain event stream.</param>
        /// <returns>New instance of domain event stream with the appended domain event stream.</returns>
        public DomainEventStream<TAggregateId> AppendDomainEventStream(IDomainEventStream<TAggregateId> streamToAppend)
        {
            if(streamToAppend == null)
            {
                throw new ArgumentNullException(nameof(streamToAppend));
            }

            if (!AggregateId.Equals(streamToAppend.AggregateId))
            {
                throw new InvalidOperationException("Cannot append domain event belonging to a different aggregate.");
            }

            if (EndVersion >= streamToAppend.BeginVersion)
            {
                throw new DomainEventVersionConflictException("Domain event streams contain some entries with overlapping versions.");
            }
            
            return new DomainEventStream<TAggregateId>(AggregateId, this.Concat(streamToAppend));
        }

        /// <summary>
        /// Get enumerator.
        /// </summary>
        /// <returns>Enumerator which yields domain events until iterated upon.</returns>
        public IEnumerator<IDomainEvent> GetEnumerator()
        {
            foreach(IDomainEvent domainEvent in _domainEvents)
            {
                yield return domainEvent;
            }
        }

        /// <summary>
        /// Get enumerator.
        /// </summary>
        /// <returns>Enumerator which yields domain events until iterated upon.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (IDomainEvent domainEvent in _domainEvents)
            {
                yield return domainEvent;
            }
        }
    }
}
