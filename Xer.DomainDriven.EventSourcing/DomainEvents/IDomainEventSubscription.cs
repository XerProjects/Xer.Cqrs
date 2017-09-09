namespace Xer.DomainDriven.EventSourcing.DomainEvents
{
    public interface IDomainEventSubscription
    {
        void Subscribe<TTopic>(IDomainEventSubscriber<TTopic> subscriber) where TTopic : IDomainEvent;
        void NotifySubscribers<TTopic>(TTopic domainEvent) where TTopic : IDomainEvent;
    }
}
