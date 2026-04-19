using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;
using RetailPulse.Web.Models;

namespace RetailPulse.Web.Controllers;

public class ProductsController : Controller
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    // LIST
    public async Task<IActionResult> Index()
    {
        var products = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .OrderBy(p => p.Name)
            .ToListAsync();

        ViewData["Title"] = "Products";
        ViewData["ActivePage"] = "Products";
        return View(products);
    }

    // CREATE — GET
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        ViewData["Title"] = "Add Product";
        ViewData["ActivePage"] = "Products";
        return View();
    }

    // CREATE — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (ModelState.IsValid)
        {
            product.CreatedAt = DateTime.UtcNow;
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Product '{product.Name}' added successfully.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateDropdowns(product.CategoryId, product.SupplierId);
        ViewData["Title"] = "Add Product";
        ViewData["ActivePage"] = "Products";
        return View(product);
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
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.ProductId) return NotFound();

        if (ModelState.IsValid)
        {
            _db.Update(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Product '{product.Name}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateDropdowns(product.CategoryId, product.SupplierId);
        ViewData["Title"] = "Edit Product";
        ViewData["ActivePage"] = "Products";
        return View(product);
    }

    // DELETE — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.IsActive = false;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Product '{product.Name}' deactivated.";
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