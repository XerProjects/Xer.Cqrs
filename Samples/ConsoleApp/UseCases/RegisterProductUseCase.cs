using System;
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
        private readonly CommandDelegator _commandDelegator;

        public override string Name => "RegisterProduct";

        public RegisterProductUseCase(CommandDelegator commandDispatcher)
        {
            _commandDelegator = commandDispatcher;
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            string id = RequestInput("Enter product ID:", input =>
            {
                if (Guid.TryParse(input, out Guid i))
                {
                    return InputValidationResult.Success;
                }

                return InputValidationResult.WithErrors("Invalid product ID.");
            });

            string productName = RequestInput("Enter product name:");

            await _commandDelegator.SendAsync(new RegisterProductCommand(Guid.Parse(id), productName));

            System.Console.WriteLine($"{productName} registered.");
        }
    }
}