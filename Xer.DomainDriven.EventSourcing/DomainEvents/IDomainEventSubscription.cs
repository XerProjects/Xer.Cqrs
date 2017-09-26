using System.Threading;
using System.Threading.Tasks;

namespace Xer.DomainDriven.EventSourcing.DomainEvents
{
    public interface IDomainEventSubscription
    {
        void Subscribe<TTopic>(IDomainEventHandler<TTopic> eventSubscriber) where TTopic : IDomainEvent;
        void Subscribe<TTopic>(IDomainEventAsyncHandler<TTopic> eventSubscriber) where TTopic : IDomainEvent;

        Task NotifySubscribersAsync<TTopic>(TTopic domainEvent, CancellationToken cancellationToken = default(CancellationToken)) where TTopic : IDomainEvent;
    }
}
