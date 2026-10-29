using Microsoft.AspNetCore.Mvc;
using OrderSystem.Services;

namespace OrderSystem.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class UsersController : Controller
{
    private readonly IUserDirectoryService _users;
    public UsersController(IUserDirectoryService users) => _users = users;

    [HttpGet]
    public IActionResult Index() => AccessDenied();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index([FromForm] string? accessCode)
    {
        // Fixed value is required by the exercise, not production authentication.
        if (!string.Equals(accessCode, "df2323eoT", StringComparison.Ordinal))
            return AccessDenied();
        return View(_users.GetAll());
    }

    private IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        return View("Access");
    }
}
