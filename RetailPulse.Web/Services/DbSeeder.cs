using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;
using RetailPulse.Web.Models;

namespace RetailPulse.Web.Services;

public static class DbSeeder
{
    public static async Task SeedSuperAdminAsync(AppDbContext db)
    {
        // Check if superadmin already has a real password hash
        var superAdmin = await db.Users
            .FirstOrDefaultAsync(u => u.Username == "superadmin");

        if (superAdmin == null) return;

        // Only update if it's still the placeholder
        if (superAdmin.PasswordHash == "$2a$11$placeholder")
        {
            superAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            await db.SaveChangesAsync();
        }
    }
}