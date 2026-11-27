namespace InventoryRD.Application.Dtos
{
    public class UpdateProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal? Price { get; set; }

        public int Stock { get; set; }

        public int CategoryId { get; set; }

        public int SupplierId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
