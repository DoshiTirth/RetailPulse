using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;
using RetailPulse.Web.Models;

namespace RetailPulse.Web.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;

    public AuthService(AppDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<(bool success, string? token, string? error)> LoginAsync(
        string username, string password)
    {
        var user = await _db.Users
            .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        if (user == null)
            return (false, null, "Invalid username or password.");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, null, "Invalid username or password.");

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Build permission list
        var permissions = user.Role.RolePermissions
            .Select(rp => $"{rp.Permission.Module}.{rp.Permission.Action}")
            .ToList();

        var token = _jwt.GenerateToken(user, permissions);
        return (true, token, null);
    }

    public async Task<bool> HasPermissionAsync(int userId, string module, string action)
    {
        return await _db.Users
            .Where(u => u.UserId == userId)
            .SelectMany(u => u.Role.RolePermissions)
            .AnyAsync(rp =>
                rp.Permission.Module == module &&
                rp.Permission.Action == action);
    }
}