using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrderSystem.Models;

public class EditPricesViewModel
{
    [Required, EnumDataType(typeof(ProductCategory))]
    public ProductCategory? Category { get; set; }
    public List<PriceEditRow> Rows { get; set; } = new();
}

public class PriceEditRow
{
    public int Id { get; set; }
    [BindNever] public string Name { get; set; } = string.Empty;
    [Required(ErrorMessage = "Вкажіть ціну")]
    [Range(typeof(decimal), "0", "100000", ErrorMessage = "Ціна має бути від 0 до 100 000 грн")]
    public decimal? Price { get; set; }
    public decimal OriginalPrice { get; set; }
}
