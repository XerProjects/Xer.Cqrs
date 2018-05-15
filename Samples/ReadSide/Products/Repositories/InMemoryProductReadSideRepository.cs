using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReadSide.Products.Repositories
{
    public class InMemoryProductReadSideRepository : IProductReadSideRepository
    {
        private List<ProductReadModel> _products = new List<ProductReadModel>();
        
        public Task AddProductAsync(ProductReadModel product, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (product == null)
            {
                throw new System.ArgumentNullException(nameof(product));
            }

            _products.Add(product);
            return Task.CompletedTask;
        }

        public Task DeleteProductByIdAsync(Guid productId, CancellationToken cancellationToken = default(CancellationToken))
        {
            _products.RemoveAll(p => p.ProductId == productId);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<ProductReadModel>> GetAllProductsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult((IReadOnlyCollection<ProductReadModel>)_products.ToList());
        }

        public Task<ProductReadModel> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var product = _products.FirstOrDefault(p => p.ProductId == productId);
            return Task.FromResult(product);
        }

        public Task<ProductReadModel> GetProductByNameAsync(string productName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var product = _products.FirstOrDefault(p => p.ProductName == productName);
            return Task.FromResult(product);
        }

        public Task UpdateProductAsync(ProductReadModel updatedProduct, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (updatedProduct == null)
            {
                throw new System.ArgumentNullException(nameof(updatedProduct));
            }

            DeleteProductByIdAsync(updatedProduct.ProductId);
            AddProductAsync(updatedProduct);
            return Task.CompletedTask;
        }
    }
}