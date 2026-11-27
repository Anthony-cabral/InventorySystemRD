using InventoryRD.Application.Core;
using InventoryRD.Application.Dtos;

namespace InventoryRD.Application.Contract
{
    public interface ICategoryService
    {
        ServiceResult<IEnumerable<CategoryDto>> GetAll();

        ServiceResult<CategoryDto> GetById(int id);

        ServiceResult<int> Create(CreateCategoryDto request);

        ServiceResult<bool> Update(int id, UpdateCategoryDto request);

        ServiceResult<bool> Delete(int id);
    }
}
