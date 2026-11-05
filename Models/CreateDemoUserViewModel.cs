using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Models;

public class CreateDemoUserViewModel
{
    [Required(ErrorMessage = "Передайте псевдонім у параметрі DisplayName адреси")]
    [StringLength(40, MinimumLength = 3, ErrorMessage = "Псевдонім має містити від 3 до 40 символів")]
    [Display(Name = "Псевдонім")]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(60, ErrorMessage = "Назва гри не може перевищувати 60 символів")]
    [Display(Name = "Улюблена гра")]
    public string? FavoriteGame { get; set; }
}
