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
