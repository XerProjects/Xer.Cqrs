using Xer.Cqrs.Events;

namespace Xer.Cqrs.EventSourcing.DomainEvents
{
    public interface IDomainEventAsyncHandler<TDomainEvent>  : IEventAsyncHandler<TDomainEvent> where TDomainEvent : IDomainEvent
    {
    }
}
