using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;
using RetailPulse.Web.Models;
using RetailPulse.Web.Models.ViewModels;
using RetailPulse.Web.Services;

namespace RetailPulse.Web.Controllers;

public class DashboardController : Controller
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    public static readonly List<(string Key, string Label)> AvailableWidgets = new()
    {
        ("kpi_revenue",        "Total Revenue"),
        ("kpi_recent_revenue", "Revenue (30 Days)"),
        ("kpi_pending_orders", "Pending Orders"),
        ("kpi_low_stock",      "Low Stock Alerts"),
        ("kpi_products",       "Active Products"),
        ("kpi_customers",      "Customers"),
        ("chart_revenue",      "Revenue Chart"),
        ("panel_low_stock",    "Low Stock Panel"),
        ("table_orders",       "Recent Orders"),
    };

    public async Task<IActionResult> Index()
    {
        var userId = PermissionService.GetUserId(User);
        var now = DateTime.UtcNow;
        var thirtyDaysAgo = now.AddDays(-30);

        // Load widget preferences
        var savedWidgets = await _db.DashboardWidgets
            .Where(w => w.UserId == userId)
            .ToListAsync();

        // Seed defaults if first time
        if (!savedWidgets.Any())
        {
            var defaults = AvailableWidgets.Select((w, i) => new DashboardWidget
            {
                UserId = userId,
                WidgetKey = w.Key,
                IsVisible = true,
                SortOrder = i
            }).ToList();

            _db.DashboardWidgets.AddRange(defaults);
            await _db.SaveChangesAsync();
            savedWidgets = defaults;
        }

        var widgetMap = savedWidgets.ToDictionary(w => w.WidgetKey);

        var orderedWidgets = AvailableWidgets
            .Select(w => new {
                Key = w.Key,
                Label = w.Label,
                IsVisible = widgetMap.TryGetValue(w.Key, out var sw) ? sw.IsVisible : true,
                SortOrder = widgetMap.TryGetValue(w.Key, out var sw2) ? sw2.SortOrder : 0
            })
            .OrderBy(w => w.SortOrder)
            .ToList();

        ViewBag.Widgets = orderedWidgets;

        // Dashboard data
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

        var recentOrders = await _db.SalesOrders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .Take(6)
            .ToListAsync();

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveWidgets(List<string> visibleWidgets)
    {
        var userId = PermissionService.GetUserId(User);

        var existing = await _db.DashboardWidgets
            .Where(w => w.UserId == userId)
            .ToListAsync();

        foreach (var widget in existing)
        {
            widget.IsVisible = visibleWidgets != null &&
                               visibleWidgets.Contains(widget.WidgetKey);
        }

        // Add any missing widgets
        var existingKeys = existing.Select(w => w.WidgetKey).ToHashSet();
        foreach (var (key, _) in AvailableWidgets.Where(w => !existingKeys.Contains(w.Key)))
        {
            _db.DashboardWidgets.Add(new DashboardWidget
            {
                UserId = userId,
                WidgetKey = key,
                IsVisible = visibleWidgets != null && visibleWidgets.Contains(key),
                SortOrder = AvailableWidgets.FindIndex(w => w.Key == key)
            });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Dashboard updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}