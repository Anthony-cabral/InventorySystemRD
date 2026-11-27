using InventoryRD.Application.Core;
using InventoryRD.Application.Dtos;

namespace InventoryRD.Application.Contract
{
    public interface ISupplierService
    {
        ServiceResult<IEnumerable<SupplierDto>> GetAll();

        ServiceResult<SupplierDto> GetById(int id);

        ServiceResult<int> Create(CreateSupplierDto request);

        ServiceResult<bool> Update(int id, UpdateSupplierDto request);

        ServiceResult<bool> Delete(int id);
    }
}
