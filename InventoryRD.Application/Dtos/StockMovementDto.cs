namespace InventoryRD.Application.Dtos
{
    public class StockMovementDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string MovementType { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}
