using Xer.Cqrs.Events;

namespace Xer.Cqrs.EventSourcing
{
    public interface IDomainEventAsyncHandler<TDomainEvent>  : IEventAsyncHandler<TDomainEvent> where TDomainEvent : IDomainEvent
    {
    }
}
