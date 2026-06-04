using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;

namespace RetailPulse.Web.Controllers.Api;

[ApiController]
[Route("api/orders")]
[Authorize(AuthenticationSchemes = "RetailPulseAuth")]
public class OrdersApiController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrdersApiController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        string? status, DateTime? from, DateTime? to)
    {
        var query = _db.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status == status);

        if (from.HasValue)
            query = query.Where(o => o.OrderDate >= from.Value);

        if (to.HasValue)
            query = query.Where(o => o.OrderDate <= to.Value.AddDays(1));

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new {
                o.OrderId,
                Customer = o.Customer.FirstName + " " + o.Customer.LastName,
                o.OrderDate,
                o.Status,
                o.TotalAmount,
                ItemCount = o.Items.Count
            }).ToListAsync();

        return Ok(new { count = orders.Count, data = orders });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _db.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.OrderId == id)
            .Select(o => new {
                o.OrderId,
                Customer = new
                {
                    o.Customer.CustomerId,
                    FullName = o.Customer.FirstName + " " + o.Customer.LastName,
                    o.Customer.Email
                },
                o.OrderDate,
                o.Status,
                o.TotalAmount,
                o.Notes,
                Items = o.Items.Select(i => new {
                    i.OrderItemId,
                    Product = i.Product.Name,
                    i.Quantity,
                    i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice
                })
            }).FirstOrDefaultAsync();

        if (order == null)
            return NotFound(new { message = $"Order {id} not found" });

        return Ok(order);
    }
}