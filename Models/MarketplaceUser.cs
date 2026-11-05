namespace OrderSystem.Models;

public record MarketplaceUser(int Id, string DisplayName, string Role, string? FavoriteGame = null);
