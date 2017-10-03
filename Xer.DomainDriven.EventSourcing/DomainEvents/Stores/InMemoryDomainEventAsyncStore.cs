using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.DomainDriven.EventSourcing.DomainEvents.Stores
{
    public class InMemoryDomainEventAsyncStore<TAggregate> : DomainEventAsyncStore<TAggregate> where TAggregate : EventSourcedAggregate
    {
        private readonly IDictionary<Guid, DomainEventStream> _domainEventStreamsByAggregateId = new Dictionary<Guid, DomainEventStream>();

        public InMemoryDomainEventAsyncStore(IDomainEventPublisher publisher) 
            : base(publisher)
        {
        }

        public override Task<DomainEventStream> GetDomainEventStreamAsync(Guid aggreggateId, CancellationToken cancellationToken = default(CancellationToken))
        {
            DomainEventStream stream;

            if (!_domainEventStreamsByAggregateId.TryGetValue(aggreggateId, out stream))
            {
                stream = DomainEventStream.Empty;
            }

            // Return a new copy, not the actual reference.
            return Task.FromResult(new DomainEventStream(stream.AggregateId, stream));
        }

        public override Task<DomainEventStream> GetDomainEventStreamAsync(Guid aggreggateId, int version, CancellationToken cancellationToken = default(CancellationToken))
        {
            DomainEventStream stream;

            if (!_domainEventStreamsByAggregateId.TryGetValue(aggreggateId, out stream))
            {
                stream = DomainEventStream.Empty;
            }

            // Return a new copy, not the actual reference.
            return Task.FromResult(new DomainEventStream(stream.AggregateId, stream.TakeWhile(e => e.AggregateVersion <= version)));
        }

        protected override Task CommitAsync(DomainEventStream domainEventStreamToCommit, CancellationToken cancellationToken = default(CancellationToken))
        {
            DomainEventStream existingStream;

            if (_domainEventStreamsByAggregateId.TryGetValue(domainEventStreamToCommit.AggregateId, out existingStream))
            {
                // Aggregate stream already exists.
                // Append and update.
                _domainEventStreamsByAggregateId[domainEventStreamToCommit.AggregateId] = existingStream.AppendDomainEventStream(domainEventStreamToCommit);
            }
            else
            {
                // Save.
                _domainEventStreamsByAggregateId.Add(domainEventStreamToCommit.AggregateId, new DomainEventStream(domainEventStreamToCommit.AggregateId, domainEventStreamToCommit));
            }

            return TaskUtility.CompletedTask;
        }
    }
}
