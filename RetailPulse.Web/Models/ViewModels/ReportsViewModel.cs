using RetailPulse.Web.Models;

namespace RetailPulse.Web.Models.ViewModels;

public class ReportsViewModel
{
    public List<MonthlyRevenuePoint> MonthlyRevenue { get; set; } = new();
    public List<TopProductPoint> TopProducts { get; set; } = new();
    public List<CategoryRevenuePoint> ByCategory { get; set; } = new();
    public List<Product> LowStock { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public string? SelectedReport { get; set; }
}

public class TopProductPoint
{
    public string ProductName { get; set; } = string.Empty;
    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }
}

public class CategoryRevenuePoint
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
}