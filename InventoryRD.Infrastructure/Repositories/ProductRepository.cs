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
    }
}
