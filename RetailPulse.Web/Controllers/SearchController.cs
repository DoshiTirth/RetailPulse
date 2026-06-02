using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;

namespace RetailPulse.Web.Controllers;

public class SearchController : Controller
{
    private readonly AppDbContext _db;

    public SearchController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Json(new List<object>());

        var results = new List<object>();

        // Products
        var products = await _db.Products
            .Where(p => p.IsActive && (p.Name.Contains(q) || p.SKU.Contains(q)))
            .Take(4)
            .Select(p => new { p.ProductId, p.Name, p.SKU })
            .ToListAsync();

        results.AddRange(products.Select(p => new {
            name = p.Name,
            type = "Product",
            meta = p.SKU,
            url = $"/Products/Edit/{p.ProductId}"
        }));

        // Customers
        var customers = await _db.Customers
            .Where(c => (c.FirstName + " " + c.LastName).Contains(q)
                     || (c.Email != null && c.Email.Contains(q)))
            .Take(3)
            .Select(c => new { c.CustomerId, c.FirstName, c.LastName, c.Email })
            .ToListAsync();

        results.AddRange(customers.Select(c => new {
            name = $"{c.FirstName} {c.LastName}",
            type = "Customer",
            meta = c.Email ?? "",
            url = $"/Customers/Detail/{c.CustomerId}"
        }));

        // Orders
        var orders = await _db.SalesOrders
            .Include(o => o.Customer)
            .Where(o => o.Customer.FirstName.Contains(q)
                     || o.Customer.LastName.Contains(q)
                     || o.OrderId.ToString().Contains(q))
            .Take(3)
            .Select(o => new { o.OrderId, o.Customer.FirstName, o.Customer.LastName, o.Status })
            .ToListAsync();

        results.AddRange(orders.Select(o => new {
            name = $"Order #{o.OrderId}",
            type = "Order",
            meta = $"{o.FirstName} {o.LastName} — {o.Status}",
            url = $"/Orders/Detail/{o.OrderId}"
        }));

        // Suppliers
        var suppliers = await _db.Suppliers
            .Where(s => s.IsActive && s.Name.Contains(q))
            .Take(2)
            .Select(s => new { s.SupplierId, s.Name, s.Country })
            .ToListAsync();

        results.AddRange(suppliers.Select(s => new {
            name = s.Name,
            type = "Supplier",
            meta = s.Country ?? "",
            url = $"/Suppliers/Edit/{s.SupplierId}"
        }));

        return Json(results);
    }
}