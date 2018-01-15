using System.Threading;
using System.Threading.Tasks;
using Domain;
using Domain.Queries;
using DomainEvents;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.QueryStack;

namespace Console.UseCases
{
    public class NotifyProductRegisteredUseCase : UseCaseBase
    {
        public override string Name => "PublishRegisteredProduct";
        private readonly IEventPublisher _eventPublisher;
        private readonly IQueryAsyncDispatcher _queryDispatcher;

        public NotifyProductRegisteredUseCase(IEventPublisher eventPublisher, IQueryAsyncDispatcher queryDispatcher)
        {
            _eventPublisher = eventPublisher;
            _queryDispatcher = queryDispatcher;
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            string productId = RequestInput("Enter ID of product to publish:", input =>
            {
                if(int.TryParse(input, out int i))
                {
                    return InputValidationResult.Success;
                }

                return InputValidationResult.WithErrors("Invalid product ID.");
            });

            Product product = await _queryDispatcher.DispatchAsync<QueryProductById, Product>(new QueryProductById(int.Parse(productId)));
            if (product == null)
            {
                System.Console.WriteLine($"Product with ID {productId} does not exist.");
            }
            
            await _eventPublisher.PublishAsync(new ProductRegisteredEvent(product.Id, product.Name));
        }
    }
}