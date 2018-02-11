using System.Threading;
using System.Threading.Tasks;
using ReadSide.Products.Repositories;
using Xer.Cqrs.QueryStack;

namespace ReadSide.Products.Queries
{
    public class QueryProductById : IQuery<ProductReadModel>
    {
        public int ProductId { get; }
        
        public QueryProductById(int productId) 
        {
            ProductId = productId;
        }
    }

    public class QueryProductByIdHandler : IQueryAsyncHandler<QueryProductById, ProductReadModel>
    {
        private readonly IProductReadSideRepository _productRepository;
        public QueryProductByIdHandler(IProductReadSideRepository productRepository)
        {
            _productRepository = productRepository;    
        }


        [QueryHandler] // To allow this method to be registered through attribute registration.
        public Task<ProductReadModel> HandleAsync(QueryProductById query, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _productRepository.GetProductByIdAsync(query.ProductId);
        }
    }
}