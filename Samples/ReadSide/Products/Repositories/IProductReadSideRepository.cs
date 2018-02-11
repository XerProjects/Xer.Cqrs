using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ReadSide.Products.Repositories
{
    public interface IProductReadSideRepository
    {
        Task<IReadOnlyCollection<ProductReadModel>> GetAllProductsAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<ProductReadModel> GetProductByIdAsync(int productId, CancellationToken cancellationToken = default(CancellationToken));
        Task<ProductReadModel> GetProductByNameAsync(string productName, CancellationToken cancellationToken = default(CancellationToken));
        Task AddProductAsync(ProductReadModel product, CancellationToken cancellationToken = default(CancellationToken));
        Task UpdateProductAsync(ProductReadModel updatedProduct, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteProductByIdAsync(int productId, CancellationToken cancellationToken = default(CancellationToken));
    }
}