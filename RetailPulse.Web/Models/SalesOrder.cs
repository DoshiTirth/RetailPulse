using System.ComponentModel.DataAnnotations;

namespace RetailPulse.Web.Models;

public class SalesOrder
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public decimal TotalAmount { get; set; } = 0;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
}