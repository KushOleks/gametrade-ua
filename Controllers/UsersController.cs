using Microsoft.AspNetCore.Mvc;
using OrderSystem.Services;
using OrderSystem.Models;

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

    [HttpGet]
    public IActionResult Create([FromQuery] CreateDemoUserViewModel model) => View(model);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateFromQuery([FromQuery] CreateDemoUserViewModel model)
    {
        if (!ModelState.IsValid) return View("Create", model);
        var user = _users.AddDemoUser(model);
        if (user is null)
        {
            ModelState.AddModelError(nameof(model.DisplayName), "Цей псевдонім уже використовується");
            return View("Create", model);
        }
        // Post/Redirect/Get: only the newly created profile is stored in this browser's TempData.
        TempData["CreatedDemoUser"] = System.Text.Json.JsonSerializer.Serialize(user);
        return RedirectToAction(nameof(ProfileCreated));
    }

    [HttpGet]
    public IActionResult ProfileCreated()
    {
        if (TempData["CreatedDemoUser"] is not string json) return NotFound();
        return View(System.Text.Json.JsonSerializer.Deserialize<MarketplaceUser>(json));
    }

    private IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        return View("Access");
    }
}
