using System;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Domain.Queries;
using Xer.Cqrs.QueryStack;

namespace Console.UseCases
{
    public class DisplayProductUseCase : UseCaseBase
    {
        private readonly IQueryAsyncDispatcher _queryDispatcher;

        public override string Name => "DisplayProduct";

        public DisplayProductUseCase(IQueryAsyncDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;    
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            string productId = RequestInput("Enter ID of product to display:", input =>
            {
                if(int.TryParse(input, out int i))
                {
                    return InputValidationResult.Success;
                }

                return InputValidationResult.WithErrors("Invalid product ID.");
            });

            Product product = await _queryDispatcher.DispatchAsync<QueryProductById, Product>(new QueryProductById(int.Parse(productId)));

            System.Console.WriteLine($"Product ID: {product.Id}, Product Name: {product.Name}, IsActive: {product.IsActive}");
        }
    }
}