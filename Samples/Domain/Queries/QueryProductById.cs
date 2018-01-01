using System.Threading;
using System.Threading.Tasks;
using Domain;
using Domain.Repositories;
using Xer.Cqrs.QueryStack;

namespace Domain.Queries
{
    public class QueryProductById : IQuery<Product>
    {
        public int ProductId { get; }
        
        public QueryProductById(int productId) 
        {
            ProductId = productId;
        }
    }

    public class QueryProductByIdHandler : IQueryAsyncHandler<QueryProductById, Product>
    {
        private readonly IProductRepository _productRepository;
        public QueryProductByIdHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;    
        }

        public Task<Product> HandleAsync(QueryProductById query, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _productRepository.GetProductByIdAsync(query.ProductId);
        }
    }
}