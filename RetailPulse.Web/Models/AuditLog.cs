using System.ComponentModel.DataAnnotations;

namespace RetailPulse.Web.Models;

public class AuditLog
{
    public int AuditId { get; set; }
    public int? UserId { get; set; }

    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Module { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    public int? EntityId { get; set; }

    [MaxLength(200)]
    public string? EntityName { get; set; }

    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User? User { get; set; }
}