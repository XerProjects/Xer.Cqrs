using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ReadSide.Products.Repositories;
using Xer.Cqrs.QueryStack;

namespace ReadSide.Products.Queries
{
    public class QueryAllProducts : IQuery<IReadOnlyCollection<ProductReadModel>>
    {
    }

    public class QueryAllProductsHandler : IQueryAsyncHandler<QueryAllProducts, IReadOnlyCollection<ProductReadModel>>
    {
        private readonly IProductReadSideRepository _productReadSideRepository;

        public QueryAllProductsHandler(IProductReadSideRepository productReadSideRepository)
        {
            _productReadSideRepository = productReadSideRepository;
        }

        [QueryHandler] // To allow this method to be registered through attribute registration.
        public Task<IReadOnlyCollection<ProductReadModel>> HandleAsync(QueryAllProducts query, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _productReadSideRepository.GetAllProductsAsync(cancellationToken);
        }
    }
}