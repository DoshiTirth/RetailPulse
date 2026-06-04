using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;
using RetailPulse.Web.Services;

namespace RetailPulse.Web.Controllers;

public class AuditController : Controller
{
    private readonly AppDbContext _db;

    public AuditController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(
        string? module, string? action, string? username,
        DateTime? from, DateTime? to, int page = 1)
    {
        if (!PermissionService.HasPermission(User, "Users", "View"))
            return RedirectToAction("AccessDenied", "Auth");

        var query = _db.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(module))
            query = query.Where(a => a.Module == module);

        if (!string.IsNullOrEmpty(action))
            query = query.Where(a => a.Action == action);

        if (!string.IsNullOrEmpty(username))
            query = query.Where(a => a.Username.Contains(username));

        if (from.HasValue)
            query = query.Where(a => a.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.CreatedAt <= to.Value.AddDays(1));

        var pageSize = 20;
        var total = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        var logs = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Populate filter dropdowns
        ViewBag.Modules = await _db.AuditLogs
            .Select(a => a.Module).Distinct().OrderBy(m => m).ToListAsync();
        ViewBag.Actions = await _db.AuditLogs
            .Select(a => a.Action).Distinct().OrderBy(a => a).ToListAsync();

        ViewBag.FilterModule = module;
        ViewBag.FilterAction = action;
        ViewBag.FilterUsername = username;
        ViewBag.FilterFrom = from?.ToString("yyyy-MM-dd");
        ViewBag.FilterTo = to?.ToString("yyyy-MM-dd");
        ViewBag.Page = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.Total = total;

        ViewData["Title"] = "Audit Log";
        ViewData["ActivePage"] = "Audit";
        return View(logs);
    }

    public async Task<IActionResult> Detail(int id)
    {
        if (!PermissionService.HasPermission(User, "Users", "View"))
            return RedirectToAction("AccessDenied", "Auth");

        var log = await _db.AuditLogs
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.AuditId == id);

        if (log == null) return NotFound();

        ViewData["Title"] = $"Audit #{id}";
        ViewData["ActivePage"] = "Audit";
        return View(log);
    }
}