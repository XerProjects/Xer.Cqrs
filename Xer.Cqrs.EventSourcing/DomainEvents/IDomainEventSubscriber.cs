namespace Xer.Cqrs.EventSourcing.DomainEvents
{
    public interface IDomainEventSubscriber<TTopic> : IDomainEventHandler<TTopic> where TTopic: IDomainEvent
    {
    }
}
