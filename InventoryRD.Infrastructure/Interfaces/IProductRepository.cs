using InventoryRD.Domain.Entities;

namespace InventoryRD.Infrastructure.Interfaces
{
    public interface IProductRepository : IBaseRepository<Products>
    {
        bool HasCategory(int categoryId);

        bool HasSupplier(int supplierId);
    }
}
