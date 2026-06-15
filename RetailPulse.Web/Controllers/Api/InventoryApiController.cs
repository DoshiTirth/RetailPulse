using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;

namespace RetailPulse.Web.Controllers.Api;

[ApiController]
[Route("api/inventory")]
[Authorize(AuthenticationSchemes = "RetailPulseAuth")]
public class InventoryApiController : ControllerBase
{
    private readonly AppDbContext _db;

    public InventoryApiController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("lowstock")]
    public async Task<IActionResult> GetLowStock()
    {
        var products = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.IsActive && p.StockQuantity <= p.ReorderLevel)
            .OrderBy(p => p.StockQuantity)
            .Select(p => new {
                p.ProductId,
                p.Name,
                p.SKU,
                Category = p.Category.Name,
                Supplier = p.Supplier.Name,
                p.StockQuantity,
                p.ReorderLevel,
                IsOutOfStock = p.StockQuantity == 0
            }).ToListAsync();

        return Ok(new { count = products.Count, data = products });
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var totalProducts = await _db.Products.CountAsync(p => p.IsActive);
        var lowStockCount = await _db.Products
            .CountAsync(p => p.IsActive && p.StockQuantity <= p.ReorderLevel);
        var outOfStock = await _db.Products
            .CountAsync(p => p.IsActive && p.StockQuantity == 0);

        return Ok(new
        {
            totalProducts,
            lowStockCount,
            outOfStock,
            wellStocked = totalProducts - lowStockCount
        });
    }
}