using System.Threading;
using System.Threading.Tasks;
using Domain.Commands;
using Xer.Cqrs.CommandStack;
using Xer.Delegator;

namespace Console.UseCases
{
    public class DeactivateProductUseCase : UseCaseBase
    {
        private readonly IMessageDelegator _commandDispatcher;

        public override string Name => "DeactivateProduct";

        public DeactivateProductUseCase(IMessageDelegator commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;    
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            string productId = RequestInput("Enter product ID:", input =>
            {
                if(int.TryParse(input, out int i))
                {
                    return InputValidationResult.Success;
                }

                return InputValidationResult.WithErrors("Invalid product ID.");
            });
            
            await _commandDispatcher.SendAsync(new DeactivateProductCommand(int.Parse(productId)));

            System.Console.WriteLine("Product deactivated.");
        }
    }
}