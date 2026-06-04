using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;
using RetailPulse.Web.Models;
using RetailPulse.Web.Services;
using System.Security.Claims;
using System.Text.Json;

namespace RetailPulse.Web.Services;

public class AuditService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public AuditService(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task LogAsync(
        string module,
        string action,
        int? entityId = null,
        string? entityName = null,
        object? oldValues = null,
        object? newValues = null)
    {
        var user = _http.HttpContext?.User;
        var userId = PermissionService.GetUserId(user!);
        var username = PermissionService.GetUsername(user!) ?? "System";
        var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString();

        var log = new AuditLog
        {
            UserId = userId > 0 ? userId : null,
            Username = username,
            Module = module,
            Action = action,
            EntityId = entityId,
            EntityName = entityName,
            OldValues = oldValues != null
                ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null
                ? JsonSerializer.Serialize(newValues) : null,
            IpAddress = ip,
            CreatedAt = DateTime.UtcNow
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}