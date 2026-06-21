using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Domain;

namespace Warehouse.Api.Data;

public class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockLevel> StockLevels => Set<StockLevel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(product =>
        {
            product.Property(p => p.SKU).HasMaxLength(64).IsRequired();
            product.Property(p => p.Name).HasMaxLength(256).IsRequired();
            product.Property(p => p.Description).HasMaxLength(256).IsRequired();

            product.HasOne(p => p.StockLevel)
                .WithOne(s => s.Product)
                .HasForeignKey<StockLevel>(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StockLevel>(stockLevel =>
            stockLevel.Property(s => s.Quantity).IsRequired()
        );
    }
}
