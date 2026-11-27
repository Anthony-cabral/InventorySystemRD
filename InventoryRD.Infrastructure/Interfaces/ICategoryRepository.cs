using InventoryRD.Domain.Entities;

namespace InventoryRD.Infrastructure.Interfaces
{
    public interface ICategoryRepository : IBaseRepository<Categories>
    {
        bool NameExists(string name, int? excludedId = null);
    }
}
