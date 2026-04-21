using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;
using RetailPulse.Web.Models;
using RetailPulse.Web.Services;

namespace RetailPulse.Web.Controllers;

public class ProductsController : Controller
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    // LIST
    public async Task<IActionResult> Index(string? category, string? status, string? search)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Name.Contains(search) || p.SKU.Contains(search));

        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.Category.Name == category);

        if (status == "active")
            query = query.Where(p => p.IsActive);
        else if (status == "inactive")
            query = query.Where(p => !p.IsActive);

        var products = await query.OrderBy(p => p.Name).ToListAsync();
        var categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();

        ViewBag.Categories = categories;
        ViewBag.FilterCat = category;
        ViewBag.FilterStatus = status;
        ViewBag.FilterSearch = search;

        ViewData["Title"] = "Products";
        ViewData["ActivePage"] = "Products";
        return View(products);
    }

    // CREATE — GET
    public async Task<IActionResult> Create()
    {
        if (!PermissionService.HasPermission(User, "Products", "Add"))
            return RedirectToAction("AccessDenied", "Auth");
        await PopulateDropdowns();
        ViewData["Title"] = "Add Product";
        ViewData["ActivePage"] = "Products";
        return View(new Product { ReorderLevel = 10, StockQuantity = 0, IsActive = true });
    }

    // CREATE — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string Name, string SKU, int CategoryId, int SupplierId,
        decimal UnitPrice, int StockQuantity, int ReorderLevel)
    {
        var existing = await _db.Products.AnyAsync(p => p.SKU == SKU);
        if (existing)
        {
            TempData["Error"] = $"SKU '{SKU}' already exists.";
            await PopulateDropdowns(CategoryId, SupplierId);
            ViewData["Title"] = "Add Product";
            ViewData["ActivePage"] = "Products";
            return View(new Product
            {
                Name = Name,
                SKU = SKU,
                CategoryId = CategoryId,
                SupplierId = SupplierId,
                UnitPrice = UnitPrice,
                StockQuantity = StockQuantity,
                ReorderLevel = ReorderLevel
            });
        }

        var product = new Product
        {
            Name = Name,
            SKU = SKU,
            CategoryId = CategoryId,
            SupplierId = SupplierId,
            UnitPrice = UnitPrice,
            StockQuantity = StockQuantity,
            ReorderLevel = ReorderLevel,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Product '{product.Name}' added successfully.";
        return RedirectToAction(nameof(Index));
    }

    // EDIT — GET
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        await PopulateDropdowns(product.CategoryId, product.SupplierId);
        ViewData["Title"] = "Edit Product";
        ViewData["ActivePage"] = "Products";
        return View(product);
    }

    // EDIT — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int ProductId, string Name, string SKU, int CategoryId, int SupplierId,
        decimal UnitPrice, int StockQuantity, int ReorderLevel, bool IsActive,
        DateTime CreatedAt)
    {
        if (!PermissionService.HasPermission(User, "Products", "Edit"))
            return RedirectToAction("AccessDenied", "Auth");
        var product = await _db.Products.FindAsync(ProductId);
        if (product == null) return NotFound();

        product.Name = Name;
        product.SKU = SKU;
        product.CategoryId = CategoryId;
        product.SupplierId = SupplierId;
        product.UnitPrice = UnitPrice;
        product.StockQuantity = StockQuantity;
        product.ReorderLevel = ReorderLevel;
        product.IsActive = IsActive;

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Product '{product.Name}' updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // TOGGLE ACTIVE — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        if (!PermissionService.HasPermission(User, "Products", "Deactivate"))
            return RedirectToAction("AccessDenied", "Auth");
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.IsActive = !product.IsActive;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Product '{product.Name}' {(product.IsActive ? "activated" : "deactivated")}.";
        return RedirectToAction(nameof(Index));
    }

    // HARD DELETE — POST (only if no orders)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!PermissionService.HasPermission(User, "Products", "Delete"))
            return RedirectToAction("AccessDenied", "Auth");
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        var hasOrders = await _db.SalesOrderItems.AnyAsync(i => i.ProductId == id);
        if (hasOrders)
        {
            TempData["Error"] = $"'{product.Name}' cannot be deleted — it has sales orders. Use Deactivate instead.";
            return RedirectToAction(nameof(Index));
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Product '{product.Name}' permanently deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns(int? categoryId = null, int? supplierId = null)
    {
        ViewBag.Categories = new SelectList(
            await _db.Categories.OrderBy(c => c.Name).ToListAsync(),
            "CategoryId", "Name", categoryId);

        ViewBag.Suppliers = new SelectList(
            await _db.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync(),
            "SupplierId", "Name", supplierId);
    }
}