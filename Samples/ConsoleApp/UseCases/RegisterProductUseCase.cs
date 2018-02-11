using System.Threading;
using System.Threading.Tasks;
using Domain.Commands;
using Domain.DomainEvents;
using Xer.Cqrs.CommandStack;
using Xer.Cqrs.EventStack;
using Xer.Delegator;

namespace ConsoleApp.UseCases
{
    public class RegisterProductUseCase : UseCaseBase
    {
        private readonly ICommandDelegator _commandDelegator;
        private readonly IEventDelegator _eventDelegator;

        public override string Name => "RegisterProduct";

        public RegisterProductUseCase(ICommandDelegator commandDispatcher, IEventDelegator eventDelegator)
        {
            _commandDelegator = commandDispatcher;
            _eventDelegator = eventDelegator;
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

            await _commandDelegator.SendAsync(new RegisterProductCommand(int.Parse(id), productName));

            System.Console.WriteLine($"{productName} registered.");
        }
    }
}