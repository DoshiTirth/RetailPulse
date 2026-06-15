using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;

namespace RetailPulse.Web.Controllers.Api;

[ApiController]
[Route("api/dashboard")]
[Authorize(AuthenticationSchemes = "RetailPulseAuth")]
public class DashboardApiController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardApiController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var totalRevenue = await _db.SalesOrders
            .Where(o => o.Status != "Cancelled")
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        var recentRevenue = await _db.SalesOrders
            .Where(o => o.Status != "Cancelled"
                     && o.OrderDate >= thirtyDaysAgo)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        var pendingOrders = await _db.SalesOrders
            .CountAsync(o => o.Status == "Pending");

        var totalProducts = await _db.Products
            .CountAsync(p => p.IsActive);

        var lowStockCount = await _db.Products
            .CountAsync(p => p.IsActive
                          && p.StockQuantity <= p.ReorderLevel);

        var totalCustomers = await _db.Customers.CountAsync();

        return Ok(new
        {
            totalRevenue,
            recentRevenue,
            pendingOrders,
            totalProducts,
            lowStockCount,
            totalCustomers,
            generatedAt = DateTime.UtcNow
        });
    }
}