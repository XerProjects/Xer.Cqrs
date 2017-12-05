using System;
using Xer.DomainDriven;

namespace Xer.EventSourcing
{
    public interface IEventSourcedAggregate<TId> : IAggregate<TId> where TId : IEquatable<TId>
    {
        /// <summary>
        /// Current version of this aggregate.
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Get an event stream of all the uncommitted domain events applied to the aggregate.
        /// </summary>
        /// <returns>Stream of uncommitted domain events.</returns>
        IDomainEventStream<TId> GetUncommitedDomainEvents();

        // <summary>
        // Clear all internally tracked domain events.
        // </summary>
        void ClearUncommitedDomainEvents();
    }
}