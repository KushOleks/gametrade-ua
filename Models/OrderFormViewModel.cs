using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace OrderSystem.Models;
public class OrderFormViewModel
{
    public int Id { get; set; }
    public string? Version { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Оберіть покупця"), Display(Name = "Покупець")]
    public int CustomerId { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Оберіть продавця"), Display(Name = "Продавець")]
    public int SellerId { get; set; }
    public List<OrderQuantityRow> Rows { get; set; } = new();
    [BindNever, ValidateNever] public IReadOnlyList<Customer> Customers { get; set; } = Array.Empty<Customer>();
    [BindNever, ValidateNever] public IReadOnlyList<Seller> Sellers { get; set; } = Array.Empty<Seller>();
}
public class OrderQuantityRow
{
    public int ProductId { get; set; }
    [Range(0, 999, ErrorMessage = "Кількість має бути від 0 до 999")]
    public int Quantity { get; set; }
    [BindNever, ValidateNever] public string Name { get; set; } = "";
    [BindNever, ValidateNever] public decimal UnitPrice { get; set; }
}
