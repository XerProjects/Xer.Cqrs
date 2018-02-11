using System;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IProductRepository
    {
        Task<Product> GetProductByIdAsync(int productId, CancellationToken cancellationToken = default(CancellationToken));
        Task SaveAsync(Product product, CancellationToken cancellationToken = default(CancellationToken));
    }
}