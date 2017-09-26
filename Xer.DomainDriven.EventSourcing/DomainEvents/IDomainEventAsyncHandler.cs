using System.Threading;
using System.Threading.Tasks;

namespace Xer.DomainDriven.EventSourcing.DomainEvents
{
    public interface IDomainEventAsyncHandler<TDomainEvent> where TDomainEvent : IDomainEvent
    {
        /// <summary>
        /// Make an operation based on the domain event asynchronously.
        /// </summary>
        /// <param name="domainEvent">Domain event.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken = default(CancellationToken));
    }
}
