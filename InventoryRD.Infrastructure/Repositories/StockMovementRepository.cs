using Microsoft.EntityFrameworkCore;
using InventoryRD.Domain.Entities;
using InventoryRD.Infrastructure.Context;
using InventoryRD.Infrastructure.Interfaces;

namespace InventoryRD.Infrastructure.Repositories
{
    public class StockMovementRepository : BaseRepository<StockMovements>, IStockMovementRepository
    {
        public StockMovementRepository(DataContext context) : base(context)
        {
        }

        public IEnumerable<StockMovements> GetAll(int? productId)
        {
            var movements = _context.StockMovements.Include(movement => movement.Product).AsNoTracking();

            if (productId.HasValue)
            {
                movements = movements.Where(movement => movement.ProductId == productId.Value);
            }

            return movements.OrderByDescending(movement => movement.CreatedAt).ThenByDescending(movement => movement.Id).ToList();
        }

        public bool HasProduct(int productId)
        {
            return _context.StockMovements.Any(movement => movement.ProductId == productId);
        }
    }
}
