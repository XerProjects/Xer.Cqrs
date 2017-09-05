using Xer.Cqrs.EventSourcing.DomainEvents.Publishers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Xer.Cqrs.EventSourcing.DomainEvents.Stores
{
    public class InMemoryDomainEventStore<TAggregate> : DomainEventStore<TAggregate> where TAggregate : EventSourcedAggregateRoot
    {
        private readonly IDictionary<Guid, IReadOnlyCollection<IDomainEvent>> _domainEventsByAggregateId = new Dictionary<Guid, IReadOnlyCollection<IDomainEvent>>();

        public InMemoryDomainEventStore(DomainEventPublisher publisher)
            : base(publisher)
        {

        }

        public override IReadOnlyCollection<IDomainEvent> GetDomainEventStream(Guid aggreggateId)
        {
            IReadOnlyCollection<IDomainEvent> domainEvents;

            if(!_domainEventsByAggregateId.TryGetValue(aggreggateId, out domainEvents))
            { 
                domainEvents = new List<IDomainEvent>().AsReadOnly();
            }

            return domainEvents;
        }

        public override ILookup<Guid, IReadOnlyCollection<IDomainEvent>> GetAllDomainEventStreamsGroupedById()
        {
            return _domainEventsByAggregateId.ToLookup(d => d.Key, d => d.Value);
        }

        protected override bool Commit(IDomainEvent domainEvent)
        {
            IReadOnlyCollection<IDomainEvent> existingDomainEvents;

            if (_domainEventsByAggregateId.TryGetValue(domainEvent.AggregateId, out existingDomainEvents))
            {
                // Append and update.
                _domainEventsByAggregateId[domainEvent.AggregateId] = AppendUncommittedDomainEventsToExisting(existingDomainEvents, domainEvent);
            }
            else // Aggregate does not yet exist.
            {
                // Get all uncommitted domain events.
                List<IDomainEvent> uncommitedDomainEvents = new List<IDomainEvent>() { domainEvent };

                // Save.
                _domainEventsByAggregateId.Add(domainEvent.AggregateId, uncommitedDomainEvents.AsReadOnly());
            }

            return true;
        }

        private IReadOnlyCollection<IDomainEvent> AppendUncommittedDomainEventsToExisting(IReadOnlyCollection<IDomainEvent> existingDomainEvents, IDomainEvent domainEventToCommit)
        {
            // Get last event.
            IDomainEvent lastEvent = existingDomainEvents.LastOrDefault();

            if(lastEvent.TimeStamp > domainEventToCommit.TimeStamp)
            {
                return existingDomainEvents;
            }

            int newListCount = existingDomainEvents.Count + 1;

            List<IDomainEvent> mergedDomainEvents = new List<IDomainEvent>(newListCount);
            mergedDomainEvents.AddRange(existingDomainEvents);
            mergedDomainEvents.Add(domainEventToCommit);

            return mergedDomainEvents.AsReadOnly();
        }
    }
}
