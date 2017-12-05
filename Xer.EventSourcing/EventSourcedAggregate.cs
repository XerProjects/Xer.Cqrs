using System;
using System.Collections.Generic;
using Xer.DomainDriven;
using Xer.EventSourcing.Exceptions;

namespace Xer.EventSourcing
{
    public abstract class EventSourcedAggregate<TId> : Aggregate<TId>, 
                                                       IEventSourcedAggregate<TId>
                                                       where TId : IEquatable<TId>
    {
        #region Declarations

        private readonly Queue<IDomainEvent> _uncommittedDomainEvents = new Queue<IDomainEvent>();
        private readonly DomainEventApplierRegistration _domainEventApplierRegistration = new DomainEventApplierRegistration();

        #endregion Declarations

        #region Properties

        /// <summary>
        /// Current version of aggregate.
        /// </summary>
        public int Version { get; private set; }


        /// <summary>
        /// Next expected version of aggregate.
        /// </summary>
        protected virtual int NextExpectedVersion => Version + 1;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="aggregateId">Id of aggregate.</param>
        public EventSourcedAggregate(TId aggregateId)
            : base(aggregateId)
        {
            RegisterDomainEventAppliers(_domainEventApplierRegistration);
        }

        /// <summary>
        /// Constructor to build this aggregate from the domain event stream.
        /// </summary>
        /// <param name="history">Domain event stream.</param>
        public EventSourcedAggregate(IDomainEventStream<TId> history)
            : this(EnsureValidDomainEventStream(history).AggregateId)
        {
            // History events are events that are already saved to event store.
            // So, just invoke the applier without tracking events.
            foreach (IDomainEvent domainEvent in history)
            {
                InvokeDomainEventApplier(domainEvent, false);
            }
        }

        #endregion Constructors

        #region IEventSourcedAggregate explicit implementations

        // Note: These methods have been implemented explicitly to avoid cluttering public API.

        /// <summary>
        /// Get an event stream of all the uncommitted domain events applied to the aggregate.
        /// </summary>
        /// <returns>Stream of uncommitted domain events.</returns>
        IDomainEventStream<TId> IEventSourcedAggregate<TId>.GetUncommitedDomainEvents()
        {
            return new DomainEventStream<TId>(Id, _uncommittedDomainEvents);
        }

        // <summary>
        // Clear all internally tracked domain events.
        // </summary>
        void IEventSourcedAggregate<TId>.ClearUncommitedDomainEvents()
        {
            _uncommittedDomainEvents.Clear();
        }

        #endregion IEventSourcedAggregate explicit implementations

        #region Protected Methods

        /// <summary>
        /// Register actions to apply domain events.
        /// </summary>
        /// <param name="applierRegistration">Domain event applier registration.</param>
        protected abstract void RegisterDomainEventAppliers(DomainEventApplierRegistration applierRegistration);

        /// <summary>
        /// Apply domain event to this entity and mark domain event for commit.
        /// </summary>
        /// <typeparam name="TDomainEvent">Type of domain event to apply.</typeparam>
        /// <param name="domainEvent">Instance of domain event to apply.</param>
        protected void ApplyChange<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent
        {
            if (domainEvent == null)
            {
                throw new ArgumentNullException(nameof(domainEvent));
            }

            if(NextExpectedVersion != domainEvent.AggregateVersion)
            {
                throw new DomainEventNotAppliedException(domainEvent,
                    $"{GetType().Name}'s expected next aggregate version is {NextExpectedVersion} but domain event has {domainEvent.AggregateVersion}.");
            }

            // Invoke and track the event to save to event store.
            InvokeDomainEventApplier(domainEvent);
        }

        #endregion Protected Methods

        #region Functions

        /// <summary>
        /// Invoke the registered action to handle the domain event.
        /// </summary>
        /// <typeparam name="TDomainEvent">Type of the domain event to handle.</typeparam>
        /// <param name="domainEvent">Domain event instance to handle.</param>
        /// <param name="markDomainEventForCommit">True, if domain event should be marked/tracked for commit. Otherwise, false - which means domain event should just be replayed.</param>
        private void InvokeDomainEventApplier<TDomainEvent>(TDomainEvent domainEvent, bool markDomainEventForCommit = true) where TDomainEvent : IDomainEvent
        {
            Action<IDomainEvent> domainEventApplier = _domainEventApplierRegistration.GetApplierFor(domainEvent);
            if (domainEventApplier == null)
            {
                throw new DomainEventNotAppliedException(domainEvent,
                    $"{GetType().Name} has no applier registered to apply domain events of type {domainEvent.GetType().Name}.");
            }

            try
            {
                domainEventApplier.Invoke(domainEvent);

                // Bump up version.
                UpdateToDomainEventVersion(domainEvent);
                
                if (markDomainEventForCommit)
                {
                    MarkAppliedDomainEventForCommit(domainEvent);
                }
            }
            catch (Exception ex)
            {
                throw new DomainEventNotAppliedException(domainEvent,
                    $"Exception occured while trying to apply domain event of type {domainEvent.GetType().Name}.",
                    ex);
            }
        }
        
        /// <summary>
        /// Add domain event to list of tracked domain events.
        /// </summary>
        /// <param name="domainEvent">Domain event instance to track.</param>
        private void MarkAppliedDomainEventForCommit(IDomainEvent domainEvent)
        {
            _uncommittedDomainEvents.Enqueue(domainEvent);
        }

        /// <summary>
        /// Update current aggregate version to match the applied domain event version.
        /// </summary>
        private void UpdateToDomainEventVersion(IDomainEvent domainEvent)
        {
            // Updated.
            Version = domainEvent.AggregateVersion;
            Updated = domainEvent.TimeStamp;
        }

        /// <summary>
        /// Ensure that the passed-in domain event stream is not null.
        /// </summary>
        /// <param name="history">Domain event stream.</param>
        /// <returns>Valid domain event stream.</returns>
        private static IDomainEventStream<TId> EnsureValidDomainEventStream(IDomainEventStream<TId> history)
        {
            if (history == null)
            {
                throw new ArgumentNullException(nameof(history));
            }

            return history;
        }

        #endregion Functions

        #region Domain Event Handler Registrations

        /// <summary>
        /// Holds the actions to be executed in handling specific types of domain event.
        /// </summary>
        protected class DomainEventApplierRegistration
        {
            private readonly IDictionary<Type, Action<IDomainEvent>> _applierByDomainEventType = new Dictionary<Type, Action<IDomainEvent>>();

            /// <summary>
            /// Register action to be executed for the domain event.
            /// </summary>
            /// <typeparam name="TDomainEvent">Type of domain event to apply.</typeparam>
            /// <param name="applier">Action to apply the domain event to the aggregate.</param>
            public void RegisterApplierFor<TDomainEvent>(Action<TDomainEvent> applier) where TDomainEvent : class, IDomainEvent
            {
                if(applier == null)
                {
                    throw new ArgumentNullException(nameof(applier));
                }

                Type domainEventType = typeof(TDomainEvent);

                if (_applierByDomainEventType.ContainsKey(domainEventType))
                {
                    throw new InvalidOperationException($"Multiple actions that apply {domainEventType.Name} domain event are registered.");
                }
                
                Action<IDomainEvent> domainEventApplier = (d) =>
                {
                    TDomainEvent domainEvent = d as TDomainEvent;
                    if(domainEvent == null)
                    {
                        throw new ArgumentException($"Invalid domain event passed to the domain event applier delegate. Delegate handles a {typeof(TDomainEvent).Name} domain event but was passed in a {d.GetType().Name} domain event.");
                    }

                    applier.Invoke(domainEvent);
                };

                _applierByDomainEventType.Add(domainEventType, domainEventApplier);
            }

            /// <summary>
            /// Get action to execute for the applied domain event.
            /// </summary>
            /// <param name="domainEvent">Domain event to apply.</param>
            /// <returns>Action that applies the domain event to the aggregate.</returns>
            public Action<IDomainEvent> GetApplierFor(IDomainEvent domainEvent)
            {
                Action<IDomainEvent> domainEventAction;

                _applierByDomainEventType.TryGetValue(domainEvent.GetType(), out domainEventAction);

                return domainEventAction;
            }
        }

        #endregion Domain Event Handler Registrations
    }
}
