using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.EventSourcing
{
    public interface IDomainEventAsyncStore<TAggregate, TAggregateId> where TAggregate : IEventSourcedAggregate<TAggregateId>
                                                                      where TAggregateId : IEquatable<TAggregateId>
    {
        /// <summary>
        /// Persist aggregate to the event store asynchronously.
        /// </summary>
        /// <param name="aggregateRoot">Aggregate to persist.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task which can be awaited asynchronously.</returns>
        Task SaveAsync(TAggregate aggregateRoot, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get all domain events of aggregate asynchronously.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All domain events for the aggregate.</returns>
        Task<IDomainEventStream<TAggregateId>> GetDomainEventStreamAsync(TAggregateId aggreggateId, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get domain events of aggregate from the beginning up to the specified version asynchronously.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="version">Target aggregate version.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Domain events for the aggregate with the specified version.</returns>
        Task<IDomainEventStream<TAggregateId>> GetDomainEventStreamAsync(TAggregateId aggreggateId, int version, CancellationToken cancellationToken = default(CancellationToken));
        
        /// <summary>
        /// Get domain events of aggregate from the beginning up to the specified version asynchronously.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="fromVersion">Aggregate version to start retrieving domain events from.</param>
        /// <param name="toVersion">Target aggregate version.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Domain events for the aggregate with the specified version.</returns>
        Task<IDomainEventStream<TAggregateId>> GetDomainEventStreamAsync(TAggregateId aggreggateId, int fromVersion, int toVersion, CancellationToken cancellationToken = default(CancellationToken));
    }
}
