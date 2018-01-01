using System.Threading;
using System.Threading.Tasks;
using Domain.Commands;
using DomainEvents;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.EventStack;

namespace Console.UseCases
{
    public class RegisterProductUseCase : UseCaseBase
    {
        private readonly ICommandAsyncDispatcher _commandDispatcher;
        private readonly IEventPublisher _eventPublisher;

        public RegisterProductUseCase(ICommandAsyncDispatcher commandDispatcher, IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
            _commandDispatcher = commandDispatcher;
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            string id = RequestInput("Enter product ID:", input =>
            {
                if(int.TryParse(input, out int i))
                {
                    return InputValidationResult.Success;
                }

                return InputValidationResult.WithErrors("Invalid product ID.");
            });

            string productName = RequestInput("Enter product name:");

            await _commandDispatcher.DispatchAsync(new RegisterProductCommand(int.Parse(id), productName));
            await _eventPublisher.PublishAsync(new ProductRegisteredEvent(int.Parse(id), productName));

            System.Console.WriteLine($"{productName} registered.");
        }
    }
}