using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.EventStack;
using Xer.Delegator;
using Xer.DomainDriven;

namespace Xer.Cqrs
{
    public class DomainEventPublisher : IDomainEventPublisher
    {
        private readonly EventDelegator _eventDelegator;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="eventDelegator">Event delegator.</param>
        public DomainEventPublisher(EventDelegator eventDelegator)
        {
            _eventDelegator = eventDelegator;
        }

        /// <summary>
        /// Publish domain events.
        /// </summary>
        /// <param name="domainEvents">Domain events to publish.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        public Task PublishAsync(IDomainEventStream domainEvents, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _eventDelegator.SendAllAsync(domainEvents, cancellationToken);
        }
    }
}