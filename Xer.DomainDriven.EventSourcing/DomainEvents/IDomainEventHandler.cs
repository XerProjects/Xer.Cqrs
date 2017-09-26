namespace Xer.DomainDriven.EventSourcing.DomainEvents
{
    public interface IDomainEventHandler<in TDomainEvent> where TDomainEvent : IDomainEvent
    {
        /// <summary>
        /// Make an operation based on the domain event.
        /// </summary>
        /// <param name="domainEvent">Domain event.</param>
        void Handle(TDomainEvent domainEvent);
    }
}
