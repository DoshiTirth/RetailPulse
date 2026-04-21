using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using RetailPulse.Web.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
namespace RetailPulse.Web.Controllers;

[AllowAnonymous]
public class AuthController : Controller
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    // LOGIN — GET
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // LOGIN — POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        var (success, token, error) = await _auth.LoginAsync(username, password);

        if (!success)
        {
            ViewData["Error"] = error;
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // Parse claims from token and sign in with cookie
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
        };

        // Re-fetch user claims properly
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                ParseClaimsFromToken(token!),
                "RetailPulseAuth"
            )
        );

        await HttpContext.SignInAsync("RetailPulseAuth", principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Dashboard");
    }

    // LOGOUT
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("RetailPulseAuth");
        return RedirectToAction("Login");
    }

    // ACCESS DENIED
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Access Denied";
        ViewData["ActivePage"] = "";
        return View();
    }

    private static IEnumerable<Claim> ParseClaimsFromToken(string token)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt.Claims;
    }
}