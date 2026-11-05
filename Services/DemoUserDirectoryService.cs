using OrderSystem.Models;

namespace OrderSystem.Services;

// Demonstration profiles only; no passwords, sign-in or privileged role assignment.
public class DemoUserDirectoryService : IUserDirectoryService
{
    private readonly object _gate = new();
    private readonly List<MarketplaceUser> _users = new()
    {
        new(1, "DemoAdmin", "Адміністратор"),
        new(2, "AlexTrade", "Продавець"),
        new(3, "GoldMaster", "Продавець"),
        new(4, "GameSeller", "Продавець")
    };
    private int _nextId = 5;

    public IReadOnlyList<MarketplaceUser> GetAll()
    {
        lock (_gate) return _users.ToArray();
    }

    public MarketplaceUser? AddDemoUser(CreateDemoUserViewModel model)
    {
        lock (_gate)
        {
            var name = model.DisplayName.Trim();
            if (_users.Any(u => string.Equals(u.DisplayName, name, StringComparison.OrdinalIgnoreCase))) return null;
            var user = new MarketplaceUser(_nextId++, name, "Користувач", model.FavoriteGame?.Trim());
            _users.Add(user);
            return user;
        }
    }
}
