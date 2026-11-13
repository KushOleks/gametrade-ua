using Microsoft.AspNetCore.Mvc;
using OrderSystem.Services;
using OrderSystem.Models;

namespace OrderSystem.Controllers;

public class HomeController : Controller
{
    private readonly IMarketplaceInfoService _marketplaceInfoService;
    private readonly IGameDirectoryService _games;

    public HomeController(IMarketplaceInfoService marketplaceInfoService, IGameDirectoryService games)
    {
        _marketplaceInfoService = marketplaceInfoService;
        _games = games;
    }

    public IActionResult Index(string? q, ProductCategory? category, bool onlyWithOffers = false)
    {
        ViewData["ProjectName"] = _marketplaceInfoService.GetProjectName();
        ViewData["Description"] = _marketplaceInfoService.GetDescription();

        var all = _games.GetGames();
        var matches = all.AsEnumerable();
        if (category.HasValue) matches = matches.Where(g => g.Categories.Contains(category.Value));
        if (onlyWithOffers) matches = matches.Where(g => g.OfferCount > 0);
        var directory = matches.ToArray();
        if (!string.IsNullOrWhiteSpace(q)) matches = directory.Where(g => g.Name.Contains(q.Trim(), StringComparison.OrdinalIgnoreCase));
        return View(new GameDirectoryViewModel
        {
            Query = q, Category = category, OnlyWithOffers = onlyWithOffers,
            Games = matches.ToArray(), DirectoryGames = directory, TotalGames = all.Count, OfferCount = all.Sum(g => g.OfferCount)
        });
    }
}