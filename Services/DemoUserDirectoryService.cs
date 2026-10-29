using OrderSystem.Models;

namespace OrderSystem.Services;

// Fictional records for Practice 3; no real account or personal data.
public class DemoUserDirectoryService : IUserDirectoryService
{
    public IReadOnlyList<MarketplaceUser> GetAll() => new MarketplaceUser[]
    {
        new(1, "DemoAdmin", "Адміністратор"),
        new(2, "AlexTrade", "Продавець"),
        new(3, "GoldMaster", "Продавець"),
        new(4, "GameSeller", "Продавець")
    };
}
