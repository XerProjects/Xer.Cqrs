using System;
using System.Threading.Tasks;

namespace AspNetCore.Domain.Repositories
{
    public interface IProductRepository
    {
        Task<Product> GetProductByIdAsync(int productId);
        Task SaveAsync(Product product);
    }
}