using System.Threading;
using System.Threading.Tasks;
using Domain;
using ReadSide.Products;
using ReadSide.Products.Queries;
using Xer.Cqrs.QueryStack;

namespace ConsoleApp.UseCases
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

            ProductReadModel product = await _queryDispatcher.DispatchAsync<QueryProductById, ProductReadModel>(new QueryProductById(int.Parse(productId)));
            if (product != null)
            {
                System.Console.WriteLine($"Product ID: {product.ProductId}, Product Name: {product.ProductName}, IsActive: {product.IsActive}");
            }
            else
            {
                System.Console.WriteLine($"Product with ID {productId} was not found.");
            }
        }
    }
}