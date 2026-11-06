using OrderSystem.Models;
namespace OrderSystem.Data;
public static class SampleData
{
    public static void Initialize(MarketplaceContext db)
    {
        // Seed only a pristine database. Never resurrect records deleted by a user.
        if (db.Sellers.Any() || db.Customers.Any() || db.Products.Any() || db.Users.Any()) return;
        var alex = new Seller { Name = "AlexTrade", City = "Київ", Description = "Ігрові предмети та скіни" };
        var gold = new Seller { Name = "GoldMaster", City = "Львів" };
        var games = new Seller { Name = "GameSeller", City = "Одеса" };
        db.Sellers.AddRange(alex, gold, games);
        db.Customers.AddRange(new Customer { FirstName = "Олена", LastName = "Коваль", City = "Київ" }, new Customer { FirstName = "Андрій", LastName = "Мельник", City = "Львів" });
        db.Products.AddRange(
            new Product { Name = "AK-47 | Redline", Game = "Counter-Strike 2", Category = ProductCategory.Skins, Price = 650, Seller = alex, Description = "Скін AK-47 Redline. Передача через Steam Trade." },
            new Product { Name = "1000 Gold", Game = "World of Warcraft", Category = ProductCategory.Currency, Price = 300, Seller = gold, Description = "1000 одиниць золота." },
            new Product { Name = "GTA V Account", Game = "GTA V", Category = ProductCategory.Accounts, Price = 900, Seller = games, Description = "Акаунт GTA V з прокачаним персонажем." });
        db.Users.AddRange(new MarketplaceUser(0, "DemoAdmin", "Адміністратор"), new MarketplaceUser(0, "AlexTrade", "Продавець"), new MarketplaceUser(0, "GoldMaster", "Продавець"), new MarketplaceUser(0, "GameSeller", "Продавець"));
        db.SaveChanges();
    }
}
