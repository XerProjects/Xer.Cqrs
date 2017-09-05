namespace Xer.Cqrs.EventSourcing.DomainEvents
{
    public interface IDomainEventHandler<in TDomainEvent> where TDomainEvent : IDomainEvent
    {
        /// <summary>
        /// Updates the read DB with the domain events.
        /// </summary>
        /// <param name="domainEventGroup"></param>
        void Handle(TDomainEvent domainEvent);
    }
}
