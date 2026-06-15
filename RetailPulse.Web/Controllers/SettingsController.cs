using Microsoft.AspNetCore.Mvc;
using RetailPulse.Web.Services;

namespace RetailPulse.Web.Controllers;

public class SettingsController : Controller
{
    private readonly EmailService _email;
    private readonly IConfiguration _config;

    public SettingsController(EmailService email, IConfiguration config)
    {
        _email = email;
        _config = config;
    }

    public IActionResult Email()
    {
        if (!PermissionService.HasPermission(User, "Users", "View"))
            return RedirectToAction("AccessDenied", "Auth");

        ViewData["Title"] = "Email Settings";
        ViewData["ActivePage"] = "Settings";

        ViewBag.Host = _config["Email:Host"];
        ViewBag.Port = _config["Email:Port"];
        ViewBag.Username = _config["Email:Username"];
        ViewBag.FromName = _config["Email:FromName"];
        ViewBag.Enabled = _config.GetValue<bool>("Email:Enabled");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestEmail(string testEmail)
    {
        if (!PermissionService.HasPermission(User, "Users", "View"))
            return RedirectToAction("AccessDenied", "Auth");

        await _email.SendAsync(testEmail, "Test",
            "RetailPulse — Test Email",
            @"<div style='font-family: sans-serif; padding: 24px;
                          background: #0F172A; color: #F8FAFC; border-radius: 12px;'>
                <h2 style='color: #22C55E;'>RetailPulse Email Test</h2>
                <p>If you received this email, your email configuration is working correctly.</p>
              </div>");

        TempData["Success"] = $"Test email sent to {testEmail}";
        return RedirectToAction(nameof(Email));
    }
}