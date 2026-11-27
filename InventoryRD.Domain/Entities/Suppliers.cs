using InventoryRD.Domain.Core;

namespace InventoryRD.Domain.Entities
{
    public class Suppliers : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
