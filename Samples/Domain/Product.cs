using System;
using System.Collections.Generic;
using Domain.DomainEvents;

namespace Domain
{
    public class Product : IDomainEventSource
    {
        private List<IDomainEvent> _pendingDomainEvents = new List<IDomainEvent>();

        public int Id { get; }
        public string Name { get; }
        public bool IsActive { get; private set; }

        public Product(int id, string name)
        {
            Id = id;
            Name = name;

            _pendingDomainEvents.Add(new ProductRegisteredEvent(Id, Name));
        }

        public void Activate()
        {
            IsActive = true;
            _pendingDomainEvents.Add(new ProductActivatedEvent(Id));
        }

        public void Deactivate()
        {
            IsActive = false;
            _pendingDomainEvents.Add(new ProductDeactivatedEvent(Id));
        }

        public IReadOnlyCollection<IDomainEvent> GetUncommittedDomainEvents()
        {
            return _pendingDomainEvents.AsReadOnly();
        }

        public void ClearUncommittedDomainEvents()
        {
            _pendingDomainEvents.Clear();
        }
    }
}