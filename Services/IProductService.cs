using OrderSystem.Models;

namespace OrderSystem.Services;

public interface IProductService
{
    IEnumerable<Product> GetAll();
    Product? GetById(int id);
    void Add(Product product);
    bool Update(Product product);
    bool Delete(int id);
}