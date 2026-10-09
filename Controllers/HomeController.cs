using Microsoft.AspNetCore.Mvc;
using OrderSystem.Services;

namespace OrderSystem.Controllers;

public class HomeController : Controller
{
    private readonly IMarketplaceInfoService _marketplaceInfoService;

    public HomeController(IMarketplaceInfoService marketplaceInfoService)
    {
        _marketplaceInfoService = marketplaceInfoService;
    }

    public IActionResult Index()
    {
        ViewData["ProjectName"] = _marketplaceInfoService.GetProjectName();
        ViewData["Description"] = _marketplaceInfoService.GetDescription();

        return View();
    }
}