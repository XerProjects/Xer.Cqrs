using System;
using System.Collections.Generic;
using Domain.DomainEvents;
using Xer.DomainDriven;

namespace Domain
{
    public class Product : AggregateRoot
    {
        public string Name { get; private set; }
        public bool IsActive { get; private set; }

        public Product(Guid id, string name)
            : base(id)
        {
            RegisterDomainEventAppliers();

            ApplyDomainEvent(new ProductRegisteredEvent(id, name));
        }
        
        public void Activate()
        {
            ApplyDomainEvent(new ProductActivatedEvent(Id));
        }

        public void Deactivate()
        {
            ApplyDomainEvent(new ProductDeactivatedEvent(Id));
        }

        private void RegisterDomainEventAppliers()
        {
            RegisterDomainEventApplier<ProductRegisteredEvent>(OnProductRegisteredEvent);
            RegisterDomainEventApplier<ProductActivatedEvent>(OnProductActivatedEvent);
            RegisterDomainEventApplier<ProductDeactivatedEvent>(OnProductDeactivatedEvent);
        }

        private void OnProductRegisteredEvent(ProductRegisteredEvent domainEvent)
        {
            Name = domainEvent.ProductName;
        }

        private void OnProductActivatedEvent(ProductActivatedEvent domainEvent)
        {
            IsActive = true;
        }

        private void OnProductDeactivatedEvent(ProductDeactivatedEvent domainEvent)
        {
            IsActive = false;
        }
    }
}