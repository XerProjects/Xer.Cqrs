using Xer.Cqrs.EventSourcing.DomainEvents.Publishers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xer.Cqrs.EventSourcing.Exceptions;

namespace Xer.Cqrs.EventSourcing.DomainEvents.Stores
{
    public class InMemoryDomainEventStore<TAggregate> : DomainEventStore<TAggregate> where TAggregate : EventSourcedAggregateRoot
    {
        private readonly IDictionary<Guid, DomainEventStream> _domainEventStreamsByAggregateId = new Dictionary<Guid, DomainEventStream>();

        public InMemoryDomainEventStore(DomainEventPublisher publisher)
            : base(publisher)
        {

        }

        public override DomainEventStream GetDomainEventStream(Guid aggreggateId)
        {
            DomainEventStream stream;

            if(!_domainEventStreamsByAggregateId.TryGetValue(aggreggateId, out stream))
            {
                stream = DomainEventStream.Empty;
            }

            // Return a new copy, not the actual reference.
            return new DomainEventStream(stream.AggregateId, stream);
        }

        public override IReadOnlyCollection<DomainEventStream> GetAllDomainEventStreams()
        {
            // Return new copies, not the actual references.
            return _domainEventStreamsByAggregateId.Select(s => new DomainEventStream(s.Key, s.Value))
                                                   .ToList()
                                                   .AsReadOnly();
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
