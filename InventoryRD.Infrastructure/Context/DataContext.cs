using Microsoft.EntityFrameworkCore;
using InventoryRD.Domain.Entities;

namespace InventoryRD.Infrastructure.Context
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Products> Products { get; set; }

        public DbSet<Categories> Categories { get; set; }

        public DbSet<Suppliers> Suppliers { get; set; }

        public DbSet<StockMovements> StockMovements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var product = modelBuilder.Entity<Products>();
            product.Property(entity => entity.Name).HasMaxLength(120);
            product.Property(entity => entity.Description).HasMaxLength(500);
            product.Property(entity => entity.Price).HasPrecision(18, 2);
            product.Property<byte[]>("RowVersion").IsRowVersion().IsRequired();
            product.HasOne<Categories>().WithMany().HasForeignKey(entity => entity.CategoryId).OnDelete(DeleteBehavior.Restrict);
            product.HasOne<Suppliers>().WithMany().HasForeignKey(entity => entity.SupplierId).OnDelete(DeleteBehavior.Restrict);
            product.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Products_Price", "[Price] >= 0");
                table.HasCheckConstraint("CK_Products_Stock", "[Stock] >= 0");
            });

            var movement = modelBuilder.Entity<StockMovements>();
            movement.Property(entity => entity.MovementType).HasMaxLength(10);
            movement.Property(entity => entity.Description).HasMaxLength(500);
            movement.HasOne(entity => entity.Product).WithMany().HasForeignKey(entity => entity.ProductId).OnDelete(DeleteBehavior.Restrict);
            movement.HasIndex(entity => new { entity.ProductId, entity.CreatedAt });
            movement.ToTable(table =>
            {
                table.HasCheckConstraint("CK_StockMovements_Quantity", "[Quantity] > 0");
                table.HasCheckConstraint("CK_StockMovements_Type", "[MovementType] IN ('Entry', 'Exit')");
            });

            var category = modelBuilder.Entity<Categories>();
            category.Property(entity => entity.Name).HasMaxLength(120).UseCollation("Latin1_General_100_CI_AS");
            category.Property(entity => entity.Description).HasMaxLength(500);
            category.HasIndex(entity => entity.Name).IsUnique();

            var supplier = modelBuilder.Entity<Suppliers>();
            supplier.Property(entity => entity.Name).HasMaxLength(120);
            supplier.Property(entity => entity.Email).HasMaxLength(254);
            supplier.Property(entity => entity.PhoneNumber).HasMaxLength(30);
        }
    }
}
