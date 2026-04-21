using System.Security.Claims;

namespace RetailPulse.Web.Services;

public static class PermissionService
{
    public static bool HasPermission(ClaimsPrincipal user, string module, string action)
    {
        var permissions = user.Claims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToHashSet();

        return permissions.Contains($"{module}.{action}");
    }

    public static string GetRoleName(ClaimsPrincipal user)
    {
        return user.Claims
            .FirstOrDefault(c => c.Type == "RoleName")?.Value ?? string.Empty;
    }

    public static int GetUserId(ClaimsPrincipal user)
    {
        var value = user.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(value, out var id) ? id : 0;
    }

    public static string GetUsername(ClaimsPrincipal user)
    {
        return user.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? string.Empty;
    }
}