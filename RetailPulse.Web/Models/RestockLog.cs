using System.ComponentModel.DataAnnotations;

namespace RetailPulse.Web.Models;

public class RestockLog
{
    public int RestockId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime RestockedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(300)]
    public string? Notes { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
}