using InventoryRD.Application.Core;
using InventoryRD.Application.Dtos;

namespace InventoryRD.Application.Contract
{
    public interface IProductService
    {
        ServiceResult<IEnumerable<ProductDto>> GetAll(string? search = null, int? categoryId = null, int? supplierId = null, bool? isActive = null);

        ServiceResult<IEnumerable<ProductDto>> GetLowStock();

        ServiceResult<ProductDto> GetById(int id);

        ServiceResult<int> Create(CreateProductDto request);

        ServiceResult<bool> Update(int id, UpdateProductDto request);

        ServiceResult<bool> Delete(int id);
    }
}
