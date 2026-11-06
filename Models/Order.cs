using System.ComponentModel.DataAnnotations.Schema;
namespace OrderSystem.Models;
public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int SellerId { get; set; }
    public Seller Seller { get; set; } = null!;
    public string Version { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAtUtc { get; set; }
    public List<OrderLine> Lines { get; set; } = new();
    [NotMapped] public decimal Total => Lines.Sum(l => l.UnitPrice * l.Quantity);
}
public class OrderLine
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string ProductName { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
