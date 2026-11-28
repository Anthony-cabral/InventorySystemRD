using InventoryRD.Domain.Core;

namespace InventoryRD.Domain.Entities
{
    public class StockMovements : BaseEntity
    {
        public int ProductId { get; set; }

        public Products Product { get; set; } = null!;

        public string MovementType { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}
