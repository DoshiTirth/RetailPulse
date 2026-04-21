using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;
using RetailPulse.Web.Models;
using RetailPulse.Web.Services;

namespace RetailPulse.Web.Controllers;

public class InventoryController : Controller
{
    private readonly AppDbContext _db;

    public InventoryController(AppDbContext db)
    {
        _db = db;
    }

    // LIST — all stock levels
    public async Task<IActionResult> Index()
    {
        var products = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.IsActive)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync();

        ViewData["Title"] = "Stock & Restock";
        ViewData["ActivePage"] = "Inventory";
        return View(products);
    }

    // RESTOCK — GET
    public async Task<IActionResult> Restock(int id)
    {
        if (!PermissionService.HasPermission(User, "Inventory", "Restock"))
            return RedirectToAction("AccessDenied", "Auth");
        var product = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null) return NotFound();

        ViewData["Title"] = $"Restock — {product.Name}";
        ViewData["ActivePage"] = "Inventory";
        return View(product);
    }

    // RESTOCK — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restock(int id, int quantity, string? notes)
    {
        if (!PermissionService.HasPermission(User, "Inventory", "Restock"))
            return RedirectToAction("AccessDenied", "Auth");
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        if (quantity <= 0)
        {
            TempData["Error"] = "Quantity must be greater than zero.";
            return RedirectToAction(nameof(Restock), new { id });
        }

        product.StockQuantity += quantity;

        var log = new RestockLog
        {
            ProductId = id,
            Quantity = quantity,
            RestockedAt = DateTime.UtcNow,
            Notes = notes
        };

        _db.RestockLogs.Add(log);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Added {quantity} units to {product.Name}. New stock: {product.StockQuantity}.";
        return RedirectToAction(nameof(Index));
    }

    // RESTOCK HISTORY
    public async Task<IActionResult> History()
    {
        var logs = await _db.RestockLogs
            .Include(r => r.Product)
            .OrderByDescending(r => r.RestockedAt)
            .ToListAsync();

        ViewData["Title"] = "Restock History";
        ViewData["ActivePage"] = "Inventory";
        return View(logs);
    }
}