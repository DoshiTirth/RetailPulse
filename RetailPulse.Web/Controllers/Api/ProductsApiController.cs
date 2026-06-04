using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;

namespace RetailPulse.Web.Controllers.Api;

[ApiController]
[Route("api/products")]
[Authorize(AuthenticationSchemes = "RetailPulseAuth")]
public class ProductsApiController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsApiController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        string? category, string? search, bool? active)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Name.Contains(search)
                                  || p.SKU.Contains(search));

        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.Category.Name == category);

        if (active.HasValue)
            query = query.Where(p => p.IsActive == active.Value);

        var products = await query.OrderBy(p => p.Name).Select(p => new {
            p.ProductId,
            p.Name,
            p.SKU,
            Category = p.Category.Name,
            Supplier = p.Supplier.Name,
            p.UnitPrice,
            p.StockQuantity,
            p.ReorderLevel,
            p.IsActive,
            IsLowStock = p.StockQuantity <= p.ReorderLevel
        }).ToListAsync();

        return Ok(new { count = products.Count, data = products });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.ProductId == id)
            .Select(p => new {
                p.ProductId,
                p.Name,
                p.SKU,
                Category = p.Category.Name,
                Supplier = p.Supplier.Name,
                p.UnitPrice,
                p.StockQuantity,
                p.ReorderLevel,
                p.IsActive,
                p.CreatedAt
            }).FirstOrDefaultAsync();

        if (product == null)
            return NotFound(new { message = $"Product {id} not found" });

        return Ok(product);
    }
}