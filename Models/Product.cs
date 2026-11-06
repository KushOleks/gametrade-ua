using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace OrderSystem.Models;

public class Product : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Вкажіть назву товару")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "Назва повинна містити від 3 до 80 символів")]
    [Display(Name = "Назва")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть гру")]
    [StringLength(60, ErrorMessage = "Назва гри не може перевищувати 60 символів")]
    [Display(Name = "Гра")]
    public string Game { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть категорію")]
    [EnumDataType(typeof(ProductCategory), ErrorMessage = "Оберіть категорію зі списку")]
    [Display(Name = "Категорія")]
    public ProductCategory? Category { get; set; }

    [Range(typeof(decimal), "0", "100000", ErrorMessage = "Ціна має бути від 0 до 100 000 грн")]
    [Display(Name = "Ціна, грн")]
    public decimal Price { get; set; }

    [StringLength(500, MinimumLength = 3, ErrorMessage = "Опис має містити від 3 до 500 символів")]
    [Display(Name = "Опис")]
    public string? Description { get; set; }

    [BindNever, ValidateNever]
    public int SellerId { get; set; }
    [BindNever, ValidateNever]
    public Seller? Seller { get; set; }
    private string _sellerName = "";

    [NotMapped]
    [Required(ErrorMessage = "Вкажіть продавця")]
    [StringLength(40, ErrorMessage = "Ім'я продавця не може перевищувати 40 символів")]
    [Display(Name = "Продавець")]
    public string SellerName { get => Seller?.Name ?? _sellerName; set => _sellerName = value; }

    [Display(Name = "Доступний")]
    public bool IsAvailable { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Description))
        {
            if (Description.Trim().Length < 3)
                yield return new ValidationResult("Опис має містити щонайменше 3 змістовні символи", new[] { nameof(Description) });
            if (string.Equals(Description.Trim(), Name.Trim(), StringComparison.OrdinalIgnoreCase))
                yield return new ValidationResult("Опис має доповнювати назву, а не повторювати її", new[] { nameof(Description) });
        }
    }
}
