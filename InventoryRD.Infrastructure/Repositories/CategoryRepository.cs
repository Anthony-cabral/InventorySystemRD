using InventoryRD.Domain.Entities;
using InventoryRD.Infrastructure.Context;
using InventoryRD.Infrastructure.Interfaces;

namespace InventoryRD.Infrastructure.Repositories
{
    public class CategoryRepository : BaseRepository<Categories>, ICategoryRepository
    {
        public CategoryRepository(DataContext context) : base(context)
        {
        }

        public bool NameExists(string name, int? excludedId = null)
        {
            return _context.Categories.Any(category => category.Name == name && category.Id != excludedId);
        }
    }
}
