using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xer.Cqrs.Events;

namespace Xer.Cqrs.EventSourcing.DomainEvents.Stores
{
    public abstract class DomainEventStore<TAggregate> : IDomainEventStore<TAggregate> where TAggregate : EventSourcedAggregate
    {
        private readonly IEventPublisher _publisher;

        public DomainEventStore(IEventPublisher publisher)
        {
            _publisher = publisher;
        }

        /// <summary>
        /// Get all domain events of aggregate.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <returns>All domain events for the aggregate.</returns>
        public abstract DomainEventStream GetDomainEventStream(Guid aggreggateId);

        /// <summary>
        /// Get domain events of aggregate from the beginning up to the specified version.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="version">Target aggregate version.</param>
        /// <returns>All domain events for the aggregate.</returns>
        public abstract DomainEventStream GetDomainEventStream(Guid aggreggateId, int version);

        /// <summary>
        /// Commit the domain event to the store.
        /// </summary>
        /// <param name="domainEventStreamToCommit">Domain event to store.</param>
        protected abstract void Commit(DomainEventStream domainEventStreamToCommit);

        /// <summary>
        /// Persist aggregate to the event store.
        /// </summary>
        /// <param name="aggregateRoot">Aggregate to persist.</param>
        public void Save(TAggregate aggregateRoot)
        {
            try
            {
                DomainEventStream domainEventsToCommit = aggregateRoot.GetUncommitedDomainEvents();
                Commit(domainEventsToCommit);
                PublishDomainEventsAsync(domainEventsToCommit);
                // Clear after committing and publishing.
                aggregateRoot.ClearUncommitedDomainEvents();
            }
            catch(Exception ex)
            {
                OnCommitError(ex);
            }
        }

        /// <summary>
        /// Publishes the domain event to event subscribers.
        /// Default implementation publishes domain events in background.
        /// </summary>
        /// <param name="eventStream">Domain events to publish.</param>
        protected virtual void PublishDomainEventsAsync(DomainEventStream eventStream)
        {
            IEnumerable<Task> publishTasks = eventStream.Select(e => _publisher.PublishAsync(e));

            Task.WhenAll(publishTasks)
            .HandleAnyExceptions(ex =>
            {
                OnPublishError(ex);
            });
        }

        /// <summary>
        /// Provide child class to handle exceptions that occur while publishing.
        /// </summary>
        /// <param name="ex">Exception that occured while publishing domain events.</param>
        protected virtual void OnPublishError(Exception ex)
        {
            throw ex;
        }

        /// <summary>
        /// Provide child class to handle exceptions that occur while committing.
        /// </summary>
        /// <param name="ex">Exception that occured while publishing domain events.</param>
        protected virtual void OnCommitError(Exception ex)
        {
            throw ex;
        }
    }
}
