using System.ComponentModel.DataAnnotations;

namespace RetailPulse.Web.Models;

public class Customer
{
    public int CustomerId { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();

    // Computed
    public string FullName => $"{FirstName} {LastName}";
}