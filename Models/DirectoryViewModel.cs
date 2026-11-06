namespace OrderSystem.Models;
public class DirectoryViewModel<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public string? Search { get; set; }
    public string Sort { get; set; } = "name";
    public int Page { get; set; }
    public int Pages { get; set; }
}
