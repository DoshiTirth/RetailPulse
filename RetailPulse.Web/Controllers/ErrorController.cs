using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RetailPulse.Web.Controllers;

[AllowAnonymous]
public class ErrorController : Controller
{
    [Route("error/{code}")]
    public IActionResult Index(int code)
    {
        Response.StatusCode = code;
        ViewData["Title"] = "Error";
        ViewData["ActivePage"] = "";
        return View("~/Views/Shared/Error.cshtml");
    }
}