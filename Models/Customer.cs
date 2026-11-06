using System.ComponentModel.DataAnnotations;
namespace OrderSystem.Models;
public class Customer
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Вкажіть ім’я"), StringLength(50), Display(Name = "Ім’я")]
    public string FirstName { get; set; } = "";
    [Required(ErrorMessage = "Вкажіть прізвище"), StringLength(50), Display(Name = "Прізвище")]
    public string LastName { get; set; } = "";
    [Required(ErrorMessage = "Вкажіть місто"), StringLength(100), Display(Name = "Місто")]
    public string City { get; set; } = "";
    public string FullName => $"{FirstName} {LastName}";
}
