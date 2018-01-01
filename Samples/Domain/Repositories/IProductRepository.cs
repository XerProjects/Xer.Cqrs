using System;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IProductRepository
    {
        Task<Product> GetProductByIdAsync(int productId);
        Task SaveAsync(Product product);
    }
}