using System.ComponentModel.DataAnnotations;

namespace RetailPulse.Web.Models;

public class DashboardWidget
{
    public int WidgetId { get; set; }
    public int UserId { get; set; }

    [Required, MaxLength(50)]
    public string WidgetKey { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
    public int SortOrder { get; set; } = 0;

    public User User { get; set; } = null!;
}