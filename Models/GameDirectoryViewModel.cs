namespace OrderSystem.Models;

public class GameDirectoryViewModel
{
    public string? Query { get; init; }
    public ProductCategory? Category { get; init; }
    public bool OnlyWithOffers { get; init; }
    public IReadOnlyList<GameDirectoryEntry> Games { get; init; } = Array.Empty<GameDirectoryEntry>();
    public IReadOnlyList<GameDirectoryEntry> DirectoryGames { get; init; } = Array.Empty<GameDirectoryEntry>();
    public int TotalGames { get; init; }
    public int OfferCount { get; init; }
}

public record GameDirectoryEntry(string Name, IReadOnlyList<ProductCategory> Categories, int OfferCount)
{
    public string Letter => char.IsLetter(Name[0]) ? Name[..1].ToUpperInvariant() : "#";
}
