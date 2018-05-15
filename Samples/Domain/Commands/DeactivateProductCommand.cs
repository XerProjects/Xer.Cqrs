using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Exceptions;
using Xer.Cqrs.CommandStack;
using Xer.DomainDriven.Repositories;
// using Xer.Cqrs.CommandStack.Attributes;

namespace Domain.Commands
{
    public class DeactivateProductCommand
    {
        public Guid ProductId { get; }

        public DeactivateProductCommand(Guid productId) 
        {
            ProductId = productId; 
        }        
    }

    /// <summary>
    /// This handler can be registered either through Container, Basic or Attribute registration.
    /// In real projects, implementing only one of the interfaces or only using the [CommandHandler] attribute should do.
    /// </summary>
    public class DeactivateProductCommandHandler : ICommandAsyncHandler<DeactivateProductCommand>
    {
        private readonly IAggregateRootRepository<Product> _productRepository;

        public DeactivateProductCommandHandler(IAggregateRootRepository<Product> productRepository)
        {
            _productRepository = productRepository;
        }

        // [CommandHandler] // To allow this method to be registered through attribute registration.
        public async Task HandleAsync(DeactivateProductCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Product product = await _productRepository.GetByIdAsync(command.ProductId);
            if (product == null)
            {
                throw new ProductNotFoundException("Product not found.");
            }

            product.Deactivate();

            await _productRepository.SaveAsync(product);
        }
    }
}