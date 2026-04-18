using System.ComponentModel.DataAnnotations;

namespace RetailPulse.Web.Models;

public class Product
{
    public int ProductId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string SKU { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public int SupplierId { get; set; }

    [Range(0, 999999)]
    public decimal UnitPrice { get; set; }

    public int StockQuantity { get; set; } = 0;
    public int ReorderLevel { get; set; } = 10;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Category Category { get; set; } = null!;
    public Supplier Supplier { get; set; } = null!;
    public ICollection<SalesOrderItem> SalesOrderItems { get; set; } = new List<SalesOrderItem>();
    public ICollection<RestockLog> RestockLogs { get; set; } = new List<RestockLog>();
}