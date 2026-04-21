using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;
using RetailPulse.Web.Models;
using RetailPulse.Web.Services;

namespace RetailPulse.Web.Controllers;

public class UsersController : Controller
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    // LIST
    public async Task<IActionResult> Index()
    {
        if (!PermissionService.HasPermission(User, "Users", "View"))
            return RedirectToAction("AccessDenied", "Auth");

        var users = await _db.Users
            .Include(u => u.Role)
            .OrderBy(u => u.Username)
            .ToListAsync();

        ViewData["Title"] = "User Management";
        ViewData["ActivePage"] = "Users";
        return View(users);
    }

    // CREATE — GET
    public async Task<IActionResult> Create()
    {
        if (!PermissionService.HasPermission(User, "Users", "Add"))
            return RedirectToAction("AccessDenied", "Auth");

        await PopulateRoles();
        ViewData["Title"] = "Add User";
        ViewData["ActivePage"] = "Users";
        return View();
    }

    // CREATE — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string username, string email,
        string password, int roleId)
    {
        if (!PermissionService.HasPermission(User, "Users", "Add"))
            return RedirectToAction("AccessDenied", "Auth");

        var existingUser = await _db.Users
            .AnyAsync(u => u.Username == username || u.Email == email);

        if (existingUser)
        {
            TempData["Error"] = "Username or email already exists.";
            await PopulateRoles(roleId);
            ViewData["Title"] = "Add User";
            ViewData["ActivePage"] = "Users";
            return View();
        }

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            RoleId = roleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"User '{username}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // EDIT — GET
    public async Task<IActionResult> Edit(int id)
    {
        if (!PermissionService.HasPermission(User, "Users", "Edit"))
            return RedirectToAction("AccessDenied", "Auth");

        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null) return NotFound();

        await PopulateRoles(user.RoleId);
        ViewData["Title"] = "Edit User";
        ViewData["ActivePage"] = "Users";
        return View(user);
    }

    // EDIT — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int userId, string username,
        string email, int roleId, string? newPassword)
    {
        if (!PermissionService.HasPermission(User, "Users", "Edit"))
            return RedirectToAction("AccessDenied", "Auth");

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.Username = username;
        user.Email = email;
        user.RoleId = roleId;

        if (!string.IsNullOrEmpty(newPassword))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

        await _db.SaveChangesAsync();
        TempData["Success"] = $"User '{username}' updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // TOGGLE ACTIVE — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        if (!PermissionService.HasPermission(User, "Users", "Deactivate"))
            return RedirectToAction("AccessDenied", "Auth");

        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        // Prevent deactivating yourself
        var currentUserId = PermissionService.GetUserId(User);
        if (user.UserId == currentUserId)
        {
            TempData["Error"] = "You cannot deactivate your own account.";
            return RedirectToAction(nameof(Index));
        }

        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"User '{user.Username}' {(user.IsActive ? "activated" : "deactivated")}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateRoles(int? selectedRoleId = null)
    {
        ViewBag.Roles = new SelectList(
            await _db.Roles.OrderBy(r => r.Name).ToListAsync(),
            "RoleId", "Name", selectedRoleId);
    }
}