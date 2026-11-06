using Microsoft.EntityFrameworkCore;
using OrderSystem.Models;
namespace OrderSystem.Data;

public class MarketplaceContext(DbContextOptions<MarketplaceContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Seller> Sellers => Set<Seller>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<MarketplaceUser> Users => Set<MarketplaceUser>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Product>().HasOne(p => p.Seller).WithMany().HasForeignKey(p => p.SellerId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Product>().Property(p => p.Price).IsConcurrencyToken();
        model.Entity<Product>().ToTable(t => t.HasCheckConstraint("CK_Product_Price", "CAST(Price AS REAL) >= 0 AND CAST(Price AS REAL) <= 100000"));
        model.Entity<Order>().Property(o => o.Version).IsConcurrencyToken();
        model.Entity<Order>().HasOne(o => o.Customer).WithMany().HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Order>().HasOne(o => o.Seller).WithMany().HasForeignKey(o => o.SellerId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<OrderLine>().HasOne(l => l.Order).WithMany(o => o.Lines).HasForeignKey(l => l.OrderId).OnDelete(DeleteBehavior.Cascade);
        model.Entity<OrderLine>().HasOne(l => l.Product).WithMany().HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<OrderLine>().HasIndex(l => new { l.OrderId, l.ProductId }).IsUnique();
        model.Entity<OrderLine>().Property(l => l.ProductName).HasMaxLength(80);
        model.Entity<OrderLine>().ToTable(t => t.HasCheckConstraint("CK_OrderLine_Quantity", "Quantity > 0 AND Quantity <= 999"));
        model.Entity<MarketplaceUser>().HasKey(u => u.Id);
        model.Entity<MarketplaceUser>().Property(u => u.DisplayName).UseCollation("NOCASE");
        model.Entity<MarketplaceUser>().HasIndex(u => u.DisplayName).IsUnique();
        model.Entity<Seller>().Property(s => s.Name).UseCollation("NOCASE");
        model.Entity<Seller>().HasIndex(s => s.Name).IsUnique();
    }
}
