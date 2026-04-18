using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;
using RetailPulse.Web.Models.ViewModels;

namespace RetailPulse.Web.Controllers;

public class DashboardController : Controller
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.UtcNow;
        var thirtyDaysAgo = now.AddDays(-30);

        var totalProducts = await _db.Products.CountAsync(p => p.IsActive);
        var totalSuppliers = await _db.Suppliers.CountAsync(s => s.IsActive);
        var totalCustomers = await _db.Customers.CountAsync();
        var lowStockCount = await _db.Products
                                .CountAsync(p => p.IsActive && p.StockQuantity <= p.ReorderLevel);

        var totalRevenue = await _db.SalesOrders
                                .Where(o => o.Status != "Cancelled")
                                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        var recentRevenue = await _db.SalesOrders
                                .Where(o => o.Status != "Cancelled" && o.OrderDate >= thirtyDaysAgo)
                                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        var pendingOrders = await _db.SalesOrders
                                .CountAsync(o => o.Status == "Pending");

        // Monthly revenue for chart (last 6 months)
        var sixMonthsAgo = now.AddMonths(-6);
        var monthlyData = await _db.SalesOrders
            .Where(o => o.Status != "Cancelled" && o.OrderDate >= sixMonthsAgo)
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .Select(g => new MonthlyRevenuePoint
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Revenue = g.Sum(o => o.TotalAmount),
                Orders = g.Count()
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        // Recent orders
        var recentOrders = await _db.SalesOrders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .Take(6)
            .ToListAsync();

        // Low stock products
        var lowStockProducts = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.IsActive && p.StockQuantity <= p.ReorderLevel)
            .OrderBy(p => p.StockQuantity)
            .Take(5)
            .ToListAsync();

        var vm = new DashboardViewModel
        {
            TotalProducts = totalProducts,
            TotalSuppliers = totalSuppliers,
            TotalCustomers = totalCustomers,
            LowStockCount = lowStockCount,
            TotalRevenue = totalRevenue,
            RecentRevenue = recentRevenue,
            PendingOrders = pendingOrders,
            MonthlyRevenue = monthlyData,
            RecentOrders = recentOrders,
            LowStockProducts = lowStockProducts
        };

        ViewData["Title"] = "Dashboard";
        ViewData["ActivePage"] = "Dashboard";
        return View(vm);
    }
}