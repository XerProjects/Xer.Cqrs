using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Exceptions;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Attributes;

namespace Domain.Commands
{
    public class ActivateProductCommand
    {
        public int ProductId { get; }

        public ActivateProductCommand(int productId) 
        {
            ProductId = productId; 
        }
    }

    /// <summary>
    /// This handler can be registered either through Container, Basic or Attribute registration.
    /// In real projects, implementing only one of the interfaces or only using the [CommandHandler] attribute should do.
    /// </summary>
    public class ActivateProductCommandHandler : ICommandAsyncHandler<ActivateProductCommand>
    {
        private readonly IProductRepository _productRepository;

        public ActivateProductCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        
        [CommandHandler] // To allow this method to be registered through attribute registration.
        public async Task HandleAsync(ActivateProductCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Product product = await _productRepository.GetProductByIdAsync(command.ProductId);
            if(product == null)
            {
                throw new ProductNotFoundException("Product not found.");
            }

            product.Activate();

            await _productRepository.SaveAsync(product);
        }
    }
}