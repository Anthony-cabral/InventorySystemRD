using InventoryRD.Domain.Entities;

namespace InventoryRD.Infrastructure.Interfaces
{
    public interface IProductRepository : IBaseRepository<Products>
    {
        IEnumerable<Products> GetAll(string? search, int? categoryId, int? supplierId, bool? isActive);

        bool HasCategory(int categoryId);

        bool HasSupplier(int supplierId);
    }
}
