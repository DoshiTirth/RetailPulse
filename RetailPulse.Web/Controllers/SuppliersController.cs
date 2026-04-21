using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;
using RetailPulse.Web.Models;
using RetailPulse.Web.Services;

namespace RetailPulse.Web.Controllers;

public class SuppliersController : Controller
{
    private readonly AppDbContext _db;

    public SuppliersController(AppDbContext db)
    {
        _db = db;
    }

    // LIST
    public async Task<IActionResult> Index()
    {
        var suppliers = await _db.Suppliers
            .Include(s => s.Products)
            .OrderBy(s => s.Name)
            .ToListAsync();

        ViewData["Title"] = "Suppliers";
        ViewData["ActivePage"] = "Suppliers";
        return View(suppliers);
    }

    // CREATE — GET
    public IActionResult Create()
    {
        if (!PermissionService.HasPermission(User, "Suppliers", "Add"))
            return RedirectToAction("AccessDenied", "Auth");
        ViewData["Title"] = "Add Supplier";
        ViewData["ActivePage"] = "Suppliers";
        return View();
    }

    // CREATE — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Supplier supplier)
    {
        if (!PermissionService.HasPermission(User, "Suppliers", "Add"))
            return RedirectToAction("AccessDenied", "Auth");
        if (ModelState.IsValid)
        {
            supplier.CreatedAt = DateTime.UtcNow;
            _db.Suppliers.Add(supplier);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Supplier '{supplier.Name}' added successfully.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Title"] = "Add Supplier";
        ViewData["ActivePage"] = "Suppliers";
        return View(supplier);
    }

    // EDIT — GET
    public async Task<IActionResult> Edit(int id)
    {
        if (!PermissionService.HasPermission(User, "Suppliers", "Edit"))
            return RedirectToAction("AccessDenied", "Auth");
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();

        ViewData["Title"] = "Edit Supplier";
        ViewData["ActivePage"] = "Suppliers";
        return View(supplier);
    }

    // EDIT — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Supplier supplier)
    {
        if (!PermissionService.HasPermission(User, "Suppliers", "Edit"))
            return RedirectToAction("AccessDenied", "Auth");
        if (id != supplier.SupplierId) return NotFound();

        if (ModelState.IsValid)
        {
            _db.Update(supplier);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Supplier '{supplier.Name}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Title"] = "Edit Supplier";
        ViewData["ActivePage"] = "Suppliers";
        return View(supplier);
    }

    // TOGGLE ACTIVE — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        if (!PermissionService.HasPermission(User, "Suppliers", "Deactivate"))
            return RedirectToAction("AccessDenied", "Auth");
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();

        supplier.IsActive = !supplier.IsActive;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Supplier '{supplier.Name}' {(supplier.IsActive ? "activated" : "deactivated")}.";
        return RedirectToAction(nameof(Index));
    }
}