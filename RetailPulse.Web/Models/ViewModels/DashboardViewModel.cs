using RetailPulse.Web.Models;

namespace RetailPulse.Web.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalProducts { get; set; }
    public int TotalSuppliers { get; set; }
    public int TotalCustomers { get; set; }
    public int LowStockCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal RecentRevenue { get; set; }
    public int PendingOrders { get; set; }

    public List<MonthlyRevenuePoint> MonthlyRevenue { get; set; } = new();
    public List<SalesOrder> RecentOrders { get; set; } = new();
    public List<Product> LowStockProducts { get; set; } = new();
}

public class MonthlyRevenuePoint
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public int Orders { get; set; }

    public string Label => new DateTime(Year, Month, 1).ToString("MMM yyyy");
}