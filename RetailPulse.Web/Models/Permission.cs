using System.ComponentModel.DataAnnotations;

namespace RetailPulse.Web.Models;

public class Permission
{
    public int PermissionId { get; set; }

    [Required, MaxLength(50)]
    public string Module { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}