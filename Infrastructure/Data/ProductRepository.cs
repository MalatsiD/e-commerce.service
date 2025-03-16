using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Data
{
    public class ProductRepository : IProductRepository
    {
        private readonly StoreContext _storeContext;

        public ProductRepository(StoreContext storeContext)
        {
            _storeContext = storeContext;
        }

        public void AddProduct(Product product)
        {
            _storeContext.Products.Add(product);
        }

        public void DeleteProduct(Product product)
        {
            _storeContext.Products.Remove(product);
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _storeContext.Products.FindAsync(id);
        }

        public async Task<IReadOnlyList<Product>> GetProductsAsync()
        {
            return await _storeContext.Products.ToListAsync();
        }

        public bool ProductExists(int id)
        {
            return _storeContext.Products.Any(x => x.Id == id);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _storeContext.SaveChangesAsync() > 0;
        }

        public void UpdateProduct(Product product)
        {
            _storeContext.Entry(product).State = EntityState.Modified;
        }
    }
}
