using OrderSystem.Models;
namespace OrderSystem.Services;
public interface IGameDirectoryService
{
    IReadOnlyList<GameDirectoryEntry> GetGames();
}
