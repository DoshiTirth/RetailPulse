using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetailPulse.Web.Data;
using RetailPulse.Web.Services;
namespace RetailPulse.Web.Controllers;

[AllowAnonymous]
public class PasswordResetController : Controller
{
    private readonly AppDbContext _db;
    private readonly EmailService _email;
    private readonly AuditService _audit;

    public PasswordResetController(AppDbContext db, EmailService email, AuditService audit)
    {
        _db = db;
        _email = email;
        _audit = audit;
    }

    // FORGOT PASSWORD — GET
    [HttpGet]
    public IActionResult Forgot()
    {
        ViewData["Title"] = "Forgot Password";
        return View();
    }

    // FORGOT PASSWORD — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Forgot(string usernameOrEmail)
    {
        ViewData["Title"] = "Forgot Password";

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Username == usernameOrEmail || u.Email == usernameOrEmail);

        // Always show success to prevent user enumeration
        if (user == null || !user.IsActive)
        {
            ViewBag.Success = "If that account exists, a reset link has been sent.";
            return View();
        }

        // Generate token
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                        .Replace("+", "-").Replace("/", "_").Replace("=", "");
        var expiry = DateTime.UtcNow.AddHours(2);

        user.ResetToken = token;
        user.ResetTokenExpiry = expiry;
        await _db.SaveChangesAsync();

        // Send email if enabled
        var resetUrl = Url.Action("Reset", "PasswordReset",
            new { token }, Request.Scheme);

        await _email.SendAsync(user.Email, user.Username,
            "RetailPulse — Password Reset",
            $@"<div style='font-family: Nunito Sans, sans-serif; max-width: 600px;
                            margin: 0 auto; background: #0F172A; color: #F8FAFC;
                            padding: 32px; border-radius: 12px;'>
                <div style='display: flex; align-items: center; gap: 12px; margin-bottom: 24px;'>
                    <div style='width: 40px; height: 40px; background: #22C55E;
                                border-radius: 10px; display: flex; align-items: center;
                                justify-content: center; font-weight: 700;
                                font-size: 16px; color: white;'>RP</div>
                    <div>
                        <div style='font-size: 18px; font-weight: 700;'>RetailPulse</div>
                        <div style='font-size: 12px; color: #64748B;'>ERP Management System</div>
                    </div>
                </div>
                <h2 style='margin-bottom: 16px;'>Password Reset Request</h2>
                <p style='color: #94A3B8; margin-bottom: 24px;'>
                    Click the button below to reset your password.
                    This link expires in 2 hours.
                </p>
                <a href='{resetUrl}' style='display: inline-block; background: #22C55E;
                    color: white; padding: 12px 24px; border-radius: 8px;
                    text-decoration: none; font-weight: 700; font-size: 14px;'>
                    Reset Password
                </a>
                <p style='margin-top: 24px; font-size: 11px; color: #475569;'>
                    If you didn't request this, ignore this email.
                    Link expires: {expiry:MMM d, yyyy h:mm tt} UTC
                </p>
               </div>");

        ViewBag.Success = "If that account exists, a reset link has been sent.";
        return View();
    }

    // RESET PASSWORD — GET
    [HttpGet]
    public async Task<IActionResult> Reset(string token)
    {
        ViewData["Title"] = "Reset Password";

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.ResetToken == token &&
            u.ResetTokenExpiry > DateTime.UtcNow);

        if (user == null)
        {
            ViewBag.Error = "This reset link is invalid or has expired.";
            return View();
        }

        ViewBag.Token = token;
        return View();
    }

    // RESET PASSWORD — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reset(string token, string newPassword, string confirmPassword)
    {
        ViewData["Title"] = "Reset Password";

        if (newPassword != confirmPassword)
        {
            ViewBag.Error = "Passwords do not match.";
            ViewBag.Token = token;
            return View();
        }

        if (newPassword.Length < 8)
        {
            ViewBag.Error = "Password must be at least 8 characters.";
            ViewBag.Token = token;
            return View();
        }

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.ResetToken == token &&
            u.ResetTokenExpiry > DateTime.UtcNow);

        if (user == null)
        {
            ViewBag.Error = "This reset link is invalid or has expired.";
            return View();
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;
        await _db.SaveChangesAsync();

        await _audit.LogAsync("Users", "PasswordReset", user.UserId, user.Username);

        TempData["Success"] = "Password reset successfully. Please log in.";
        return RedirectToAction("Login", "Auth");
    }
}