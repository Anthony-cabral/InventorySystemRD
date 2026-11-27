using InventoryRD.Domain.Entities;

namespace InventoryRD.Infrastructure.Interfaces
{
    public interface IStockMovementRepository : IBaseRepository<StockMovements>
    {
        IEnumerable<StockMovements> GetAll(int? productId);

        bool HasProduct(int productId);
    }
}
