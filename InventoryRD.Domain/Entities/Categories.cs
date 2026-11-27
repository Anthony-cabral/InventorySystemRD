using InventoryRD.Domain.Core;

namespace InventoryRD.Domain.Entities
{
    public class Categories : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
