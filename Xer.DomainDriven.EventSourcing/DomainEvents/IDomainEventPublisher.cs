namespace Xer.DomainDriven.EventSourcing.DomainEvents
{
    public interface IDomainEventPublisher
    {
        void Publish(IDomainEvent domainEvent);
    }
}
