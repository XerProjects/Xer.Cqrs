using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.CommandStack;
using Xer.DomainDriven.Repositories;
// using Xer.Cqrs.CommandStack.Attributes;

namespace Domain.Commands
{
    public class RegisterProductCommand
    {
        public Guid ProductId { get; }
        public string ProductName { get; }
        
        public RegisterProductCommand(Guid productId, string productName) 
        {
            ProductId = productId;
            ProductName = productName;
        }
    }

    /// <summary>
    /// This handler can be registered either through Container, Basic or Attribute registration.
    /// In real projects, implementing only one of the interfaces or only using the [CommandHandler] attribute should do.
    /// </summary>
    public class RegisterProductCommandHandler : ICommandAsyncHandler<RegisterProductCommand>
    {
        private readonly IAggregateRootRepository<Product> _productRepository;

        public RegisterProductCommandHandler(IAggregateRootRepository<Product> productRepository)
        {
            _productRepository = productRepository;
        }

        // [CommandHandler] // To allow this method to be registered through attribute registration.
        public Task HandleAsync(RegisterProductCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _productRepository.SaveAsync(new Product(command.ProductId, command.ProductName));
        }
    }
}