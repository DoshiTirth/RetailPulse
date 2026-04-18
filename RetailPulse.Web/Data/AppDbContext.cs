using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Models;

namespace RetailPulse.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<SalesOrder> SalesOrders { get; set; }
    public DbSet<SalesOrderItem> SalesOrderItems { get; set; }
    public DbSet<RestockLog> RestockLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Supplier
        modelBuilder.Entity<Supplier>()
            .Property(s => s.Name).IsRequired().HasMaxLength(150);

        // Category
        modelBuilder.Entity<Category>()
            .Property(c => c.Name).IsRequired().HasMaxLength(100);

        // Product — SKU must be unique
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.SKU).IsUnique();

        modelBuilder.Entity<Product>()
            .Property(p => p.UnitPrice).HasColumnType("decimal(10,2)");

        // SalesOrder
        modelBuilder.Entity<SalesOrder>()
            .Property(o => o.TotalAmount).HasColumnType("decimal(12,2)");

        modelBuilder.Entity<SalesOrder>()
            .Property(o => o.Status).HasMaxLength(20);

        // SalesOrderItem — LineTotal is computed in DB, ignore in EF
        modelBuilder.Entity<SalesOrderItem>()
            .Property(i => i.UnitPrice).HasColumnType("decimal(10,2)");

        modelBuilder.Entity<SalesOrderItem>()
            .Ignore(i => i.UnitPrice); // will be set manually, not computed by EF

        // Restore UnitPrice — we need it, just not as computed
        modelBuilder.Entity<SalesOrderItem>()
            .Property(i => i.UnitPrice).HasColumnType("decimal(10,2)").IsRequired();

        // RestockLog
        modelBuilder.Entity<RestockLog>()
            .Property(r => r.Notes).HasMaxLength(300);
    }
}