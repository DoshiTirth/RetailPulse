using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;
using RetailPulse.Web.Models;
using RetailPulse.Web.Services;

namespace RetailPulse.Web.Controllers;

public class CategoriesController : Controller
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    // LIST
    public async Task<IActionResult> Index()
    {
        var categories = await _db.Categories
            .Include(c => c.Products)
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewData["Title"] = "Categories";
        ViewData["ActivePage"] = "Categories";
        return View(categories);
    }

    // CREATE — GET
    public IActionResult Create()
    {
        if (!PermissionService.HasPermission(User, "Categories", "Add"))
            return RedirectToAction("AccessDenied", "Auth");
        ViewData["Title"] = "Add Category";
        ViewData["ActivePage"] = "Categories";
        return View();
    }

    // CREATE — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (!PermissionService.HasPermission(User, "Categories", "Add"))
            return RedirectToAction("AccessDenied", "Auth");
        if (ModelState.IsValid)
        {
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Category '{category.Name}' added successfully.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Title"] = "Add Category";
        ViewData["ActivePage"] = "Categories";
        return View(category);
    }

    // EDIT — GET
    public async Task<IActionResult> Edit(int id)
    {
        if (!PermissionService.HasPermission(User, "Categories", "Edit"))
            return RedirectToAction("AccessDenied", "Auth");
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();

        ViewData["Title"] = "Edit Category";
        ViewData["ActivePage"] = "Categories";
        return View(category);
    }

    // EDIT — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
        if (!PermissionService.HasPermission(User, "Categories", "Edit"))
            return RedirectToAction("AccessDenied", "Auth");
        if (id != category.CategoryId) return NotFound();

        if (ModelState.IsValid)
        {
            _db.Update(category);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Category '{category.Name}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Title"] = "Edit Category";
        ViewData["ActivePage"] = "Categories";
        return View(category);
    }

    // DELETE — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!PermissionService.HasPermission(User, "Categories", "Delete"))
            return RedirectToAction("AccessDenied", "Auth");
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return NotFound();

        var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
        {
            TempData["Error"] = "Cannot delete category — it has products assigned to it.";
            return RedirectToAction(nameof(Index));
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Category '{category.Name}' deleted.";
        return RedirectToAction(nameof(Index));
    }
}