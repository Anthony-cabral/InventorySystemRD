using InventoryRD.Application.Core;
using InventoryRD.Application.Dtos;

namespace InventoryRD.Application.Contract
{
    public interface IStockMovementService
    {
        ServiceResult<IEnumerable<StockMovementDto>> GetAll(int? productId = null);

        ServiceResult<int> Create(CreateStockMovementDto request);
    }
}
