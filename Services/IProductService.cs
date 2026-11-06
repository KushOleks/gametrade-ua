using OrderSystem.Models;

namespace OrderSystem.Services;

public interface IProductService
{
    IEnumerable<Product> GetAll();
    IEnumerable<string> GetSellerNames() => GetAll().Select(p => p.SellerName).Distinct().OrderBy(n => n);
    Product? GetById(int id);
    void Add(Product product);
    bool Update(Product product);
    bool Delete(int id);
    bool UpdatePrices(ProductCategory category, IReadOnlyList<PriceEditRow> rows);
}