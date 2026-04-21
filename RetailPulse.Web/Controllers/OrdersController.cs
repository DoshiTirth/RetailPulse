using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;
using RetailPulse.Web.Models;
using RetailPulse.Web.Models.ViewModels;
using RetailPulse.Web.Services;

namespace RetailPulse.Web.Controllers;

public class OrdersController : Controller
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db)
    {
        _db = db;
    }

    // LIST
    public async Task<IActionResult> Index()
    {
        var orders = await _db.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        ViewData["Title"] = "Sales Orders";
        ViewData["ActivePage"] = "Orders";
        return View(orders);
    }

    // DETAIL
    public async Task<IActionResult> Detail(int id)
    {
        var order = await _db.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null) return NotFound();

        ViewData["Title"] = $"Order #{order.OrderId}";
        ViewData["ActivePage"] = "Orders";
        return View(order);
    }

    // CREATE — GET
    public async Task<IActionResult> Create()
    {
        if (!PermissionService.HasPermission(User, "Orders", "Add"))
            return RedirectToAction("AccessDenied", "Auth");
        await PopulateDropdowns();
        ViewData["Title"] = "New Order";
        ViewData["ActivePage"] = "Orders";
        return View(new CreateOrderViewModel());
    }

    // CREATE — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOrderViewModel vm)
    {
        if (!PermissionService.HasPermission(User, "Orders", "Add"))
            return RedirectToAction("AccessDenied", "Auth");
        if (ModelState.IsValid)
        {
            var order = new SalesOrder
            {
                CustomerId = vm.CustomerId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                Notes = vm.Notes
            };

            _db.SalesOrders.Add(order);
            await _db.SaveChangesAsync();

            decimal total = 0;
            foreach (var line in vm.Items.Where(i => i.ProductId > 0 && i.Quantity > 0))
            {
                var product = await _db.Products.FindAsync(line.ProductId);
                if (product == null) continue;

                var item = new SalesOrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    UnitPrice = product.UnitPrice
                };

                _db.SalesOrderItems.Add(item);
                total += product.UnitPrice * line.Quantity;
            }

            order.TotalAmount = total;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Order #{order.OrderId} created successfully.";
            return RedirectToAction(nameof(Detail), new { id = order.OrderId });
        }

        await PopulateDropdowns(vm.CustomerId);
        ViewData["Title"] = "New Order";
        ViewData["ActivePage"] = "Orders";
        return View(vm);
    }

    // UPDATE STATUS — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        if (!PermissionService.HasPermission(User, "Orders", "UpdateStatus"))
            return RedirectToAction("AccessDenied", "Auth");
        var order = await _db.SalesOrders.FindAsync(id);
        if (order == null) return NotFound();

        order.Status = status;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Order #{id} marked as {status}.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    private async Task PopulateDropdowns(int? customerId = null)
    {
        ViewBag.Customers = new SelectList(
            await _db.Customers.OrderBy(c => c.LastName).ToListAsync(),
            "CustomerId", "FullName", customerId);

        ViewBag.Products = await _db.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new { p.ProductId, p.Name, p.UnitPrice })
            .ToListAsync();
    }
}