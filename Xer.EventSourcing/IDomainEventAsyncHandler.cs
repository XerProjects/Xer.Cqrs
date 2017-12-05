using Xer.Cqrs.EventStack;

namespace Xer.EventSourcing
{
    public interface IDomainEventAsyncHandler<TDomainEvent>  : IEventAsyncHandler<TDomainEvent> where TDomainEvent : IDomainEvent
    {
    }
}
