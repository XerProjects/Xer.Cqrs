using System;
using System.Collections.Generic;
using Xer.Cqrs.EventSourcing.Exceptions;
using Xer.DomainDriven;

namespace Xer.Cqrs.EventSourcing
{
    public abstract class EventSourcedAggregate : Aggregate, IEventSourcedAggregate
    {
        #region Declarations

        private readonly Queue<IDomainEvent> _uncommittedDomainEvents = new Queue<IDomainEvent>();
        private readonly DomainEventApplierRegistration _domainEventApplierRegistration = new DomainEventApplierRegistration();

        #endregion Declarations

        #region Properties

        /// <summary>
        /// Current version of this aggregate.
        /// </summary>
        public int Version { get; private set; }


        /// <summary>
        /// Next expected version of this aggregate.
        /// </summary>
        protected virtual int NextExpectedVersion => Version + 1;

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="aggregateId">Id of this entity.</param>
        public EventSourcedAggregate(Guid aggregateId)
            : base(aggregateId)
        {
            RegisterDomainEventAppliers(_domainEventApplierRegistration);
        }

        /// <summary>
        /// Constructor to build this entity from the domain event stream.
        /// </summary>
        /// <param name="history">Domain event stream.</param>
        public EventSourcedAggregate(DomainEventStream history)
            : this(Guid.Empty)
        {
            if (history == null)
            {
                throw new ArgumentNullException(nameof(history));
            }

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
        DomainEventStream IEventSourcedAggregate.GetUncommitedDomainEvents()
        {
            return new DomainEventStream(Id, _uncommittedDomainEvents);
        }

        // <summary>
        // Clear all internally tracked domain events.
        // </summary>
        void IEventSourcedAggregate.ClearUncommitedDomainEvents()
        {
            _uncommittedDomainEvents.Clear();
        }

        #endregion IEventSourcedAggregate explicit implementations

        #region Protected Methods

        /// <summary>
        /// Register actions to apply certain domain events.
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

            // Invoke and track the event to save to event store.
            InvokeDomainEventApplier(domainEvent);
        }

        /// <summary>
        /// Invoke the registered action to handle the domain event.
        /// </summary>
        /// <typeparam name="TDomainEvent">Type of the domain event to handle.</typeparam>
        /// <param name="domainEvent">Domain event instance to handle.</param>
        /// <param name="markDomainEventForCommit">True, if domain event should be marked/tracked for commit. Otherwise, false.</param>
        protected void InvokeDomainEventApplier<TDomainEvent>(TDomainEvent domainEvent, bool markDomainEventForCommit = true) where TDomainEvent : IDomainEvent
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
                UpdateToNextVersion();
                
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

        #endregion Protected Methods

        #region Functions
        
        /// <summary>
        /// Add domain event to list of tracked domain events.
        /// </summary>
        /// <param name="domainEvent">Domain event instance to track.</param>
        private void MarkAppliedDomainEventForCommit(IDomainEvent domainEvent)
        {
            _uncommittedDomainEvents.Enqueue(domainEvent);
        }

        /// <summary>
        /// Update current aggregate version to the next expected version and update the updated timestamp.
        /// </summary>
        private void UpdateToNextVersion()
        {
            Version = NextExpectedVersion;

            // Updated.
            Updated = DateTime.Now;
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
