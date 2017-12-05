using Xer.Cqrs.EventStack;

namespace Xer.EventSourcing
{
    public interface IDomainEventHandler<in TDomainEvent> : IEventHandler<TDomainEvent> where TDomainEvent : IDomainEvent
    {
    }
}
