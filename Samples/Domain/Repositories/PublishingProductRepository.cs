using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.EventStack;
using Xer.Delegator;

namespace Domain.Repositories
{
    public class PublishingProductRepository : IProductRepository
    {
        private readonly EventDelegator _eventDelegator;
        private readonly IProductRepository _inner;

        public PublishingProductRepository(IProductRepository inner, EventDelegator eventDelegator)
        {
            _inner = inner ?? throw new System.ArgumentNullException(nameof(inner));
            _eventDelegator = eventDelegator ?? throw new System.ArgumentNullException(nameof(eventDelegator));
        }
        
        public Task<Product> GetProductByIdAsync(int productId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _inner.GetProductByIdAsync(productId, cancellationToken);
        }

        public async Task SaveAsync(Product product, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get copy.
            IReadOnlyCollection<IDomainEvent> uncommittedDomainEvents = product.GetUncommittedDomainEvents();

            // Do actual save.
            await _inner.SaveAsync(product, cancellationToken);

            // Send each domain events to handlers.
            List<Task> publishDomainEventTasks = uncommittedDomainEvents.Select(e => _eventDelegator.SendAsync(e, cancellationToken)).ToList();

            // Complete when all events have completed.
            await Task.WhenAll(publishDomainEventTasks);
        }
    }
}