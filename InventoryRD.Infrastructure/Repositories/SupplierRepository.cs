using InventoryRD.Domain.Entities;
using InventoryRD.Infrastructure.Context;
using InventoryRD.Infrastructure.Interfaces;

namespace InventoryRD.Infrastructure.Repositories
{
    public class SupplierRepository : BaseRepository<Suppliers>, ISupplierRepository
    {
        public SupplierRepository(DataContext context) : base(context)
        {
        }
    }
}
