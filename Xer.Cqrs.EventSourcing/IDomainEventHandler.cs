using Xer.Cqrs.Events;

namespace Xer.Cqrs.EventSourcing
{
    public interface IDomainEventHandler<in TDomainEvent> : IEventHandler<TDomainEvent> where TDomainEvent : IDomainEvent
    {
    }
}
