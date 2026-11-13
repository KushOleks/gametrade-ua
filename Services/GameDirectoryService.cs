using OrderSystem.Models;
namespace OrderSystem.Services;

public class GameDirectoryService(IProductService products) : IGameDirectoryService
{
    // Independently maintained directory. Titles are plain text, without game artwork or logos.
    private static readonly (string Name, ProductCategory[] Categories)[] Directory =
    {
        ("Albion Online", new[] { ProductCategory.Currency, ProductCategory.Items, ProductCategory.Services }),
        ("Apex Legends", new[] { ProductCategory.Accounts, ProductCategory.Services }),
        ("Black Desert Online", new[] { ProductCategory.Currency, ProductCategory.Accounts, ProductCategory.Services }),
        ("Counter-Strike 2", new[] { ProductCategory.Skins, ProductCategory.Items, ProductCategory.Services }),
        ("Diablo IV", new[] { ProductCategory.Currency, ProductCategory.Items, ProductCategory.Services }),
        ("Dota 2", new[] { ProductCategory.Items, ProductCategory.Skins, ProductCategory.Services }),
        ("EA Sports FC", new[] { ProductCategory.Currency, ProductCategory.Accounts, ProductCategory.Services }),
        ("Final Fantasy XIV", new[] { ProductCategory.Currency, ProductCategory.Items, ProductCategory.Services }),
        ("Fortnite", new[] { ProductCategory.Accounts, ProductCategory.Services }),
        ("Genshin Impact", new[] { ProductCategory.Accounts, ProductCategory.Services }),
        ("GTA V", new[] { ProductCategory.Accounts, ProductCategory.Currency, ProductCategory.Services }),
        ("Guild Wars 2", new[] { ProductCategory.Currency, ProductCategory.Items, ProductCategory.Services }),
        ("Honkai: Star Rail", new[] { ProductCategory.Accounts, ProductCategory.Services }),
        ("League of Legends", new[] { ProductCategory.Accounts, ProductCategory.Services }),
        ("Lineage 2", new[] { ProductCategory.Currency, ProductCategory.Items, ProductCategory.Services }),
        ("Minecraft", new[] { ProductCategory.Accounts, ProductCategory.Services }),
        ("Path of Exile", new[] { ProductCategory.Currency, ProductCategory.Items, ProductCategory.Services }),
        ("Rust", new[] { ProductCategory.Skins, ProductCategory.Items, ProductCategory.Services }),
        ("Valorant", new[] { ProductCategory.Accounts, ProductCategory.Services }),
        ("World of Warcraft", new[] { ProductCategory.Currency, ProductCategory.Items, ProductCategory.Services })
    };

    public IReadOnlyList<GameDirectoryEntry> GetGames()
    {
        var inventory = products.GetAll().ToArray();
        // Include games introduced by sellers automatically, and count only real available listings.
        var names = Directory.Select(g => g.Name).Concat(inventory.Select(p => p.Game.Trim()))
            .Where(n => n.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(n => n, StringComparer.OrdinalIgnoreCase);
        return names.Select(name =>
        {
            var listings = inventory.Where(p => p.Game.Trim().Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray();
            var defaults = Directory.FirstOrDefault(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Categories ?? Array.Empty<ProductCategory>();
            var categories = defaults.Concat(listings.Where(p => p.Category.HasValue).Select(p => p.Category!.Value)).Distinct().OrderBy(c => c).ToArray();
            return new GameDirectoryEntry(name, categories, listings.Count(p => p.IsAvailable));
        }).ToArray();
    }
}
