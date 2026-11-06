using Microsoft.EntityFrameworkCore;
using OrderSystem.Models;
using OrderSystem.Data;
namespace OrderSystem.Services;
public class EfUserDirectoryService(MarketplaceContext db) : IUserDirectoryService
{
    public IReadOnlyList<MarketplaceUser> GetAll() => db.Users.AsNoTracking().OrderBy(u => u.Id).ToArray();
    public MarketplaceUser? AddDemoUser(CreateDemoUserViewModel model)
    {
        var name = model.DisplayName.Trim();
        if (db.Users.Any(u => u.DisplayName == name)) return null;
        var user = new MarketplaceUser(0, name, "Користувач", model.FavoriteGame?.Trim());
        db.Users.Add(user);
        try { db.SaveChanges(); return user; }
        catch (DbUpdateException) { db.ChangeTracker.Clear(); return null; }
    }
}
