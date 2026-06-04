using Microsoft.AspNetCore.Mvc;
using RetailPulse.Web.Services;

namespace RetailPulse.Web.Controllers.Api;

public class ApiDocsController : Controller
{
    public IActionResult Index()
    {
        if (!PermissionService.HasPermission(User, "Users", "View"))
            return RedirectToAction("AccessDenied", "Auth");

        ViewData["Title"] = "API Documentation";
        ViewData["ActivePage"] = "ApiDocs";
        return View("~/Views/ApiDocs/Index.cshtml");
    }
}