using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace OrderSystem.Models;

public enum ProductCategory
{
    [Display(Name = "Ігрова валюта")] Currency = 1,
    [Display(Name = "Предмети")] Items = 2,
    [Display(Name = "Акаунти")] Accounts = 3,
    [Display(Name = "Послуги")] Services = 4,
    [Display(Name = "Скіни")] Skins = 5
}

public static class ProductCategories
{
    public static string Label(this ProductCategory? category) => category.HasValue
        ? typeof(ProductCategory).GetMember(category.Value.ToString()).FirstOrDefault()?
            .GetCustomAttribute<DisplayAttribute>()?.Name ?? "Оберіть категорію"
        : "Оберіть категорію";

    // Keep links from Practice 3 (with Ukrainian category names) working.
    public static bool TryParse(string? value, out ProductCategory category)
    {
        foreach (var item in Enum.GetValues<ProductCategory>())
        {
            if (string.Equals(value, item.ToString(), StringComparison.OrdinalIgnoreCase)
                || value == ((ProductCategory?)item).Label())
            {
                category = item;
                return true;
            }
        }
        category = default;
        return false;
    }
}
