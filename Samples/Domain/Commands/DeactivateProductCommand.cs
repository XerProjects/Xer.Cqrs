using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Exceptions;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Attributes;

namespace Domain.Commands
{
    public class DeactivateProductCommand
    {
        public int ProductId { get; }

        public DeactivateProductCommand(int productId) 
        {
            ProductId = productId; 
        }        
    }

    /// <summary>
    /// This handler can be registered either through Container, Basic or Attribute registration.
    /// In real projects, implementing only one of the interfaces or only using the [CommandHandler] attribute should do.
    /// </summary>
    public class DeactivateProductCommandHandler : ICommandHandler<DeactivateProductCommand>,
                                                   ICommandAsyncHandler<DeactivateProductCommand>
    {
        private readonly IProductRepository _productRepository;

        public DeactivateProductCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task HandleAsync(DeactivateProductCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Product product = await _productRepository.GetProductByIdAsync(command.ProductId);
            if(product == null)
            {
                throw new ProductNotFoundException("Product not found.");
            }

            product.Deactivate();

            await _productRepository.SaveAsync(product);
        }

        public void Handle(DeactivateProductCommand command)
        {
            HandleAsync(command).GetAwaiter().GetResult();
        }

        [CommandHandler]
        public Task HandleDeactivateProductCommandAsync(DeactivateProductCommand command)
        {
            return HandleAsync(command);
        }
    }
}