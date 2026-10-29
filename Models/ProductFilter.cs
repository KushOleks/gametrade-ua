using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Models;

public class ProductFilter : IValidatableObject
{
    public string? Search { get; set; }
    public string? Game { get; set; }
    public string? Category { get; set; }

    [Range(0, 1000000, ErrorMessage = "Ціна має бути від 0 до 1 000 000 грн")]
    public decimal? MinPrice { get; set; }

    [Range(0, 1000000, ErrorMessage = "Ціна має бути від 0 до 1 000 000 грн")]
    public decimal? MaxPrice { get; set; }
    public bool OnlyAvailable { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice > MaxPrice)
            yield return new ValidationResult("Мінімальна ціна не може перевищувати максимальну", new[] { nameof(MinPrice) });
    }
}

public class ProductCatalogViewModel
{
    public ProductFilter Filter { get; init; } = new();
    public IReadOnlyList<Product> Products { get; init; } = Array.Empty<Product>();
    public IReadOnlyList<string> Games { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Categories { get; init; } = Array.Empty<string>();
}
