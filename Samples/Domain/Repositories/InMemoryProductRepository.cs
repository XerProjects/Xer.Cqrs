using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public class InMemoryProductRepository : IProductRepository
    {
        List<Product> _products = new List<Product>();

        public Task<Product> GetProductByIdAsync(int productId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var product = _products.FirstOrDefault(p => p.Id == productId);
            return Task.FromResult(product);
        }

        public Task SaveAsync(Product product, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }
            
            // Remove existing product.
            _products.RemoveAll(p => p.Id == product.Id);
            // Add updated product.
            _products.Add(product);

            return Task.CompletedTask;
        }
    }
}