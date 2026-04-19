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

        // ─── Primary Keys (explicit) ──────────────────────────
        modelBuilder.Entity<Supplier>().HasKey(s => s.SupplierId);
        modelBuilder.Entity<Category>().HasKey(c => c.CategoryId);
        modelBuilder.Entity<Product>().HasKey(p => p.ProductId);
        modelBuilder.Entity<Customer>().HasKey(c => c.CustomerId);
        modelBuilder.Entity<SalesOrder>().HasKey(o => o.OrderId);
        modelBuilder.Entity<SalesOrderItem>().HasKey(i => i.OrderItemId);
        modelBuilder.Entity<RestockLog>().HasKey(r => r.RestockId);
        modelBuilder.Entity<RestockLog>().ToTable("RestockLog");

        // ─── Supplier ─────────────────────────────────────────
        modelBuilder.Entity<Supplier>()
            .Property(s => s.Name).IsRequired().HasMaxLength(150);

        // ─── Category ─────────────────────────────────────────
        modelBuilder.Entity<Category>()
            .Property(c => c.Name).IsRequired().HasMaxLength(100);

        // ─── Product ──────────────────────────────────────────
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.SKU).IsUnique();

        modelBuilder.Entity<Product>()
            .Property(p => p.UnitPrice).HasColumnType("decimal(10,2)");

        // ─── SalesOrder ───────────────────────────────────────
        modelBuilder.Entity<SalesOrder>()
            .Property(o => o.TotalAmount).HasColumnType("decimal(12,2)");

        modelBuilder.Entity<SalesOrder>()
            .Property(o => o.Status).HasMaxLength(20);

        // ─── SalesOrderItem ───────────────────────────────────
        modelBuilder.Entity<SalesOrderItem>()
            .Property(i => i.UnitPrice).HasColumnType("decimal(10,2)");

        // ─── RestockLog ───────────────────────────────────────
        modelBuilder.Entity<RestockLog>()
            .Property(r => r.Notes).HasMaxLength(300);

        // ─── Relationships (explicit FK mapping) ─────────────────
        modelBuilder.Entity<SalesOrderItem>()
            .HasOne(i => i.SalesOrder)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId);

        modelBuilder.Entity<SalesOrderItem>()
            .HasOne(i => i.Product)
            .WithMany(p => p.SalesOrderItems)
            .HasForeignKey(i => i.ProductId);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Supplier)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SupplierId);

        modelBuilder.Entity<SalesOrder>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.SalesOrders)
            .HasForeignKey(o => o.CustomerId);

        modelBuilder.Entity<RestockLog>()
            .HasOne(r => r.Product)
            .WithMany(p => p.RestockLogs)
            .HasForeignKey(r => r.ProductId);
    }
}