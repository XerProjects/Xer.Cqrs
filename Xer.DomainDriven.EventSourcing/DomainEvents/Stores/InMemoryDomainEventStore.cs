using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Xer.DomainDriven.EventSourcing.DomainEvents.Stores
{
    public class InMemoryDomainEventStore<TAggregate> : DomainEventStore<TAggregate> where TAggregate : EventSourcedAggregate
    {
        private readonly IDictionary<Guid, DomainEventStream> _domainEventStreamsByAggregateId = new Dictionary<Guid, DomainEventStream>();

        public InMemoryDomainEventStore(IDomainEventPublisher publisher)
            : base(publisher)
        {

        }

        public override DomainEventStream GetDomainEventStream(Guid aggreggateId)
        {
            DomainEventStream stream;

            if (!_domainEventStreamsByAggregateId.TryGetValue(aggreggateId, out stream))
            {
                stream = DomainEventStream.Empty;
            }

            // Return a new copy, not the actual reference.
            return new DomainEventStream(stream.AggregateId, stream);
        }

        public override DomainEventStream GetDomainEventStream(Guid aggreggateId, int version)
        {
            DomainEventStream stream;

            if(!_domainEventStreamsByAggregateId.TryGetValue(aggreggateId, out stream))
            {
                stream = DomainEventStream.Empty;
            }

            // Return a new copy, not the actual reference.
            return new DomainEventStream(stream.AggregateId, stream.TakeWhile(e => e.AggregateVersion <= version));
        }

        protected override void Commit(DomainEventStream domainEventStreamToCommit)
        {
            DomainEventStream existingStream;

            if (_domainEventStreamsByAggregateId.TryGetValue(domainEventStreamToCommit.AggregateId, out existingStream))
            {
                // Aggregate stream already exists.
                // Append and update.
                _domainEventStreamsByAggregateId[domainEventStreamToCommit.AggregateId] = existingStream.AppendStream(domainEventStreamToCommit);
            }
            else 
            {
                // Save.
                _domainEventStreamsByAggregateId.Add(domainEventStreamToCommit.AggregateId, new DomainEventStream(domainEventStreamToCommit.AggregateId, domainEventStreamToCommit));
            }
        }
    }
}
