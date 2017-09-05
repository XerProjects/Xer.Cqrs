using Xer.Cqrs.EventSourcing.DomainEvents;
using System;
using System.Collections.Generic;

namespace Xer.Cqrs.EventSourcing
{
    public abstract class EventSourcedAggregateRoot : Entity
    {
        private readonly List<IDomainEvent> _uncommittedDomainEvents = new List<IDomainEvent>();
        private readonly DomainEventApplierRegistration _domainEventApplierRegistration = new DomainEventApplierRegistration();

        public EventSourcedAggregateRoot(Guid aggregateId) 
            : base(aggregateId)
        {
            RegisterDomainEventAppliers(_domainEventApplierRegistration);
        }

        protected EventSourcedAggregateRoot(IEnumerable<IDomainEvent> history)
            : this(Guid.Empty)
        {
            if(history == null)
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

        public void ClearUncommitedDomainEvents()
        {
            _uncommittedDomainEvents.Clear();
        }

        public IReadOnlyCollection<IDomainEvent> GetUncommittedDomainEvents()
        {
            return _uncommittedDomainEvents.AsReadOnly();
        }

        protected abstract void RegisterDomainEventAppliers(DomainEventApplierRegistration applierRegistration);
        
        protected void MarkAppliedDomainEventForCommit(IDomainEvent domainEvent)
        {
            _uncommittedDomainEvents.Add(domainEvent);
        }

        protected void ApplyChange<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent
        {
            // Invoke and track the event to save to event store.
            InvokeDomainEventApplier(domainEvent);
        }

        protected void ApplyChanges<TDomainEvent>(IEnumerable<IDomainEvent> domainEvents) where TDomainEvent : IDomainEvent
        {
            foreach (IDomainEvent domainEvent in domainEvents)
            {
                ApplyChange(domainEvent);
            }
        }

        protected void InvokeDomainEventApplier<TDomainEvent>(TDomainEvent domainEvent, bool markDomainEventForCommit = true) where TDomainEvent : IDomainEvent
        {
            if (domainEvent == null)
            {
                throw new ArgumentNullException(nameof(domainEvent));
            }

            Action<IDomainEvent> domainEventApplier = _domainEventApplierRegistration.GetDomainEventApplier(domainEvent);
            if (domainEventApplier == null)
            {
                throw new InvalidOperationException($"{GetType().Name} is not configured to support domain event of type {domainEvent.GetType().Name}");
            }

            domainEventApplier.Invoke(domainEvent);

            if(markDomainEventForCommit)
            {
                MarkAppliedDomainEventForCommit(domainEvent);

                // Updated.
                Updated = domainEvent.TimeStamp;
            }
        }

        #region Domain Event Handler Registrations

        protected class DomainEventApplierRegistration
        {
            private readonly Dictionary<Type, Action<IDomainEvent>> _appliersByDomainEventType = new Dictionary<Type, Action<IDomainEvent>>();

            public void RegisterDomainEventApplier<TDomainEvent>(Action<TDomainEvent> applier) where TDomainEvent : IDomainEvent
            {
                Type domainEventType = typeof(TDomainEvent);

                Action<IDomainEvent> domainEventApplier = new Action<IDomainEvent>((d) => applier.Invoke((TDomainEvent)d));

                _appliersByDomainEventType.Add(domainEventType, domainEventApplier);
            }

            public Action<IDomainEvent> GetDomainEventApplier(IDomainEvent domainEvent)
            {
                Action<IDomainEvent> actionHandler;

                _appliersByDomainEventType.TryGetValue(domainEvent.GetType(), out actionHandler);

                return actionHandler;
            }
        }

        #endregion Domain Event Handler Registrations
    }
}
