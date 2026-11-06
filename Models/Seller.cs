using System.ComponentModel.DataAnnotations;
namespace OrderSystem.Models;
public class Seller
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Вкажіть назву продавця"), StringLength(40), Display(Name = "Назва продавця")]
    public string Name { get; set; } = "";
    [StringLength(100), Display(Name = "Місто")]
    public string? City { get; set; }
    [StringLength(500), Display(Name = "Опис")]
    public string? Description { get; set; }
}
