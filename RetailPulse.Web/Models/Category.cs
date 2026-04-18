using System.ComponentModel.DataAnnotations;

namespace RetailPulse.Web.Models;

public class Category
{
    public int CategoryId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Navigation
    public ICollection<Product> Products { get; set; } = new List<Product>();
}