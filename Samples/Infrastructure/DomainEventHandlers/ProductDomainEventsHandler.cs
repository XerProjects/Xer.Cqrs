using System.Threading;
using System.Threading.Tasks;
using Domain.DomainEvents;
using ReadSide.Products;
using ReadSide.Products.Repositories;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Attributes;

namespace Infrastructure.DomainEventHandlers
{
    /// <summary>
    /// Contains handlers for product domain events.
    /// </summary>
    public class ProductDomainEventsHandler : IEventAsyncHandler<ProductRegisteredEvent>,
                                              IEventAsyncHandler<ProductActivatedEvent>,
                                              IEventAsyncHandler<ProductDeactivatedEvent>
    {
        private readonly IProductReadSideRepository _productReadSideRepository;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="productReadSideRepository">Product read side repository.</param>
        public ProductDomainEventsHandler(IProductReadSideRepository productReadSideRepository)
        {
            _productReadSideRepository = productReadSideRepository;
        }

        /// <summary>
        /// Handle ProductRegisteredEvent.
        /// </summary>
        /// <param name="productRegisteredEvent">ProductRegisteredEvent instance.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task which can be awaited for completion.</returns>
        [EventHandler] // To allow this method to be registered through attribute registration.
        public Task HandleAsync(ProductRegisteredEvent productRegisteredEvent, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Add product to read-side repository if event is received.
            return _productReadSideRepository.AddProductAsync(new ProductReadModel()
            {
                ProductId = productRegisteredEvent.ProductId,
                ProductName = productRegisteredEvent.ProductName
            });
        }

        /// <summary>
        /// Handle ProductActivatedEvent.
        /// </summary>
        /// <param name="productActivatedEvent">ProductActivatedEvent instance.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task which can be awaited for completion.</returns>
        [EventHandler] // To allow this method to be registered through attribute registration.
        public async Task HandleAsync(ProductActivatedEvent productActivatedEvent, CancellationToken cancellationToken = default(CancellationToken))
        {
            var product = await _productReadSideRepository.GetProductByIdAsync(productActivatedEvent.ProductId, cancellationToken);

            // Set read-side product to active.
            product.IsActive = true;

            await _productReadSideRepository.UpdateProductAsync(product, cancellationToken);
        }

        /// <summary>
        /// Handle ProductDeactivatedEvent.
        /// </summary>
        /// <param name="productDeactivatedEvent">ProductDeactivatedEvent instance.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task which can be awaited for completion.</returns>
        [EventHandler] // To allow this method to be registered through attribute registration.
        public async Task HandleAsync(ProductDeactivatedEvent productDeactivatedEvent, CancellationToken cancellationToken = default(CancellationToken))
        {
            var product = await _productReadSideRepository.GetProductByIdAsync(productDeactivatedEvent.ProductId, cancellationToken);

            // Set read-side product to inactive.
            product.IsActive = false;

            await _productReadSideRepository.UpdateProductAsync(product, cancellationToken);
        }
    }
}