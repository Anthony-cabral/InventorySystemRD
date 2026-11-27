using InventoryRD.Domain.Entities;
using InventoryRD.Infrastructure.Context;
using InventoryRD.Infrastructure.Interfaces;

namespace InventoryRD.Infrastructure.Repositories
{
    public class ProductRepository : BaseRepository<Products>, IProductRepository
    {
        public ProductRepository(DataContext context) : base(context)
        {
        }

        public IEnumerable<Products> GetAll(string? search, int? categoryId, int? supplierId, bool? isActive)
        {
            var products = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                products = products.Where(product => product.Name.Contains(term) || product.Description.Contains(term));
            }

            if (categoryId.HasValue)
            {
                products = products.Where(product => product.CategoryId == categoryId.Value);
            }

            if (supplierId.HasValue)
            {
                products = products.Where(product => product.SupplierId == supplierId.Value);
            }

            if (isActive.HasValue)
            {
                products = products.Where(product => product.IsActive == isActive.Value);
            }

            return products.OrderBy(product => product.Name).ThenBy(product => product.Id).ToList();
        }

        public bool HasCategory(int categoryId)
        {
            return _context.Products.Any(product => product.CategoryId == categoryId);
        }

        public bool HasSupplier(int supplierId)
        {
            return _context.Products.Any(product => product.SupplierId == supplierId);
        }
    }
}
