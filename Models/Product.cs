using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Models;

public class Product
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
    [RegularExpression("^(Ігрова валюта|Предмети|Акаунти|Послуги|Скіни)$", ErrorMessage = "Оберіть категорію зі списку")]
    [Display(Name = "Категорія")]
    public string Category { get; set; } = string.Empty;

    [Range(0.01, 1000000, ErrorMessage = "Ціна повинна бути більшою за 0")]
    [Display(Name = "Ціна, грн")]
    public decimal Price { get; set; }

    [StringLength(500, ErrorMessage = "Опис не може перевищувати 500 символів")]
    [Display(Name = "Опис")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Вкажіть продавця")]
    [StringLength(40, ErrorMessage = "Ім'я продавця не може перевищувати 40 символів")]
    [Display(Name = "Продавець")]
    public string SellerName { get; set; } = string.Empty;

    [Display(Name = "Доступний")]
    public bool IsAvailable { get; set; } = true;
}