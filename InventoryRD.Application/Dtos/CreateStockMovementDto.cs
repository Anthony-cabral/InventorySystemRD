namespace InventoryRD.Application.Dtos
{
    public class CreateStockMovementDto
    {
        public int ProductId { get; set; }

        public string MovementType { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public string? Description { get; set; }
    }
}
