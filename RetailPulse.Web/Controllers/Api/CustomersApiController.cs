using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;

namespace RetailPulse.Web.Controllers.Api;

[ApiController]
[Route("api/customers")]
[Authorize(AuthenticationSchemes = "RetailPulseAuth")]
public class CustomersApiController : ControllerBase
{
    private readonly AppDbContext _db;

    public CustomersApiController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(string? search)
    {
        var query = _db.Customers
            .Include(c => c.SalesOrders)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(c =>
                (c.FirstName + " " + c.LastName).Contains(search) ||
                (c.Email != null && c.Email.Contains(search)));

        var customers = await query
            .OrderBy(c => c.LastName)
            .Select(c => new {
                c.CustomerId,
                FullName = c.FirstName + " " + c.LastName,
                c.Email,
                c.Phone,
                c.City,
                OrderCount = c.SalesOrders.Count,
                TotalSpent = c.SalesOrders.Sum(o => o.TotalAmount),
                c.CreatedAt
            }).ToListAsync();

        return Ok(new { count = customers.Count, data = customers });
    }
}