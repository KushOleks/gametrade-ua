using OrderSystem.Models;

namespace OrderSystem.Services;

public interface IUserDirectoryService
{
    IReadOnlyList<MarketplaceUser> GetAll();
}
