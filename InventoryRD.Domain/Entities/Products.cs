using InventoryRD.Domain.Core;

namespace InventoryRD.Domain.Entities
{
    public class Products : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Stock { get; set; }

        public int CategoryId { get; set; }

        public int SupplierId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
