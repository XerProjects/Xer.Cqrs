using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Exceptions;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.CommandStack.Attributes;

namespace Domain.Commands
{
    public class RegisterProductCommand : Command
    {
        public int ProductId { get; }
        public string ProductName { get; }
        
        public RegisterProductCommand(int productId, string productName) 
        {
            ProductId = productId;
            ProductName = productName;
        }
    }

    /// <summary>
    /// This handler can be registered either through Container, Basic or Attribute registration.
    /// In real projects, implementing only one of the interfaces or only using the [CommandHandler] attribute should do.
    /// </summary>
    public class RegisterProductCommandHandler : ICommandAsyncHandler<RegisterProductCommand>,
                                                 ICommandHandler<RegisterProductCommand>
    {
        private readonly IProductRepository _productRepository;

        public RegisterProductCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public Task HandleAsync(RegisterProductCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _productRepository.SaveAsync(new Product(command.ProductId, command.ProductName));
        }

        public void Handle(RegisterProductCommand command)
        {
            HandleAsync(command).GetAwaiter().GetResult();
        }

        [CommandHandler]
        public Task HandleRegisterProductCommandAsync(RegisterProductCommand command, CancellationToken cancellationToken)
        {
            return HandleAsync(command, cancellationToken);
        }
    }
}