using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;
using RetailPulse.Web.Models;
using RetailPulse.Web.Services;

namespace RetailPulse.Web.Controllers;

public class CustomersController : Controller
{
    private readonly AppDbContext _db;

    public CustomersController(AppDbContext db)
    {
        _db = db;
    }

    // LIST
    public async Task<IActionResult> Index()
    {
        var customers = await _db.Customers
            .Include(c => c.SalesOrders)
            .OrderBy(c => c.LastName)
            .ToListAsync();

        ViewData["Title"] = "Customers";
        ViewData["ActivePage"] = "Customers";
        return View(customers);
    }

    // CREATE — GET
    public IActionResult Create()
    {
        if (!PermissionService.HasPermission(User, "Customers", "Add"))
            return RedirectToAction("AccessDenied", "Auth");
        ViewData["Title"] = "Add Customer";
        ViewData["ActivePage"] = "Customers";
        return View();
    }

    // CREATE — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer customer)
    {
        if (!PermissionService.HasPermission(User, "Customers", "Add"))
            return RedirectToAction("AccessDenied", "Auth");
        if (ModelState.IsValid)
        {
            customer.CreatedAt = DateTime.UtcNow;
            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Customer '{customer.FullName}' added successfully.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Title"] = "Add Customer";
        ViewData["ActivePage"] = "Customers";
        return View(customer);
    }

    // EDIT — GET
    public async Task<IActionResult> Edit(int id)
    {
        if (!PermissionService.HasPermission(User, "Customers", "Edit"))
            return RedirectToAction("AccessDenied", "Auth");
        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        ViewData["Title"] = "Edit Customer";
        ViewData["ActivePage"] = "Customers";
        return View(customer);
    }

    // EDIT — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Customer customer)
    {
        if (!PermissionService.HasPermission(User, "Customers", "Edit"))
            return RedirectToAction("AccessDenied", "Auth");
        if (id != customer.CustomerId) return NotFound();

        if (ModelState.IsValid)
        {
            _db.Update(customer);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Customer '{customer.FullName}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        ViewData["Title"] = "Edit Customer";
        ViewData["ActivePage"] = "Customers";
        return View(customer);
    }

    // DETAIL — order history
    public async Task<IActionResult> Detail(int id)
    {
        var customer = await _db.Customers
            .Include(c => c.SalesOrders)
            .FirstOrDefaultAsync(c => c.CustomerId == id);

        if (customer == null) return NotFound();

        ViewData["Title"] = customer.FullName;
        ViewData["ActivePage"] = "Customers";
        return View(customer);
    }
}