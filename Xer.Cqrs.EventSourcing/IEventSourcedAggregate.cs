using Xer.DomainDriven;

namespace Xer.Cqrs.EventSourcing
{
    public interface IEventSourcedAggregate : IAggregate
    {
        /// <summary>
        /// Current version of this aggregate.
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Get an event stream of all the uncommitted domain events applied to the aggregate.
        /// </summary>
        /// <returns>Stream of uncommitted domain events.</returns>
        DomainEventStream GetUncommitedDomainEvents();

        // <summary>
        // Clear all internally tracked domain events.
        // </summary>
        void ClearUncommitedDomainEvents();
    }
}