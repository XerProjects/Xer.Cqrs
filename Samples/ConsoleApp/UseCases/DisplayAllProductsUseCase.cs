using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReadSide.Products;
using ReadSide.Products.Queries;
using ReadSide.Products.Repositories;
using Xer.Cqrs.QueryStack;

namespace ConsoleApp.UseCases
{
    public class DisplayAllProductsUseCase : UseCaseBase
    {
        private readonly IQueryAsyncDispatcher _queryDispatcher;

        public override string Name => "DisplayAllProducts";

        public DisplayAllProductsUseCase(IQueryAsyncDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IReadOnlyCollection<ProductReadModel> products = 
                await _queryDispatcher.DispatchAsync<QueryAllProducts, IReadOnlyCollection<ProductReadModel>>(new QueryAllProducts(), cancellationToken);
            
            if(products.Count > 0)
            {
                foreach(ProductReadModel product in products)
                    Console.WriteLine($"Product ID: {product.ProductId}, Product Name: {product.ProductName}, IsActive: {product.IsActive}");
            }
            else
            {
                System.Console.WriteLine("No products found.");
            }
        }
    }
}