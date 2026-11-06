using Microsoft.EntityFrameworkCore;
using OrderSystem.Data;
using OrderSystem.Models;
namespace OrderSystem.Services;

public class EfProductService(MarketplaceContext db) : IProductService
{
    public IEnumerable<string> GetSellerNames() => db.Sellers.AsNoTracking().OrderBy(s => s.Name).Select(s => s.Name).ToArray();
    public IEnumerable<Product> GetAll() => db.Products.AsNoTracking().Include(p => p.Seller).OrderBy(p => p.Id).ToArray();
    public Product? GetById(int id) => db.Products.AsNoTracking().Include(p => p.Seller).SingleOrDefault(p => p.Id == id);
    private Seller ResolveSeller(string name) => db.Sellers.SingleOrDefault(s => s.Name == name.Trim()) ?? new Seller { Name = name.Trim() };
    public void Add(Product product)
    {
        product.Id = 0;
        product.Seller = ResolveSeller(product.SellerName);
        db.Products.Add(product);
        db.SaveChanges();
    }
    public bool Update(Product product)
    {
        var current = db.Products.Find(product.Id);
        if (current is null) return false;
        current.Name = product.Name;
        current.Game = product.Game;
        current.Category = product.Category;
        current.Price = product.Price;
        current.Description = product.Description;
        current.IsAvailable = product.IsAvailable;
        current.Seller = ResolveSeller(product.SellerName);
        db.SaveChanges();
        return true;
    }
    public bool Delete(int id)
    {
        var product = db.Products.Find(id);
        if (product is null || db.OrderLines.Any(l => l.ProductId == id)) return false;
        db.Products.Remove(product);
        try { db.SaveChanges(); return true; }
        catch (DbUpdateException) { db.ChangeTracker.Clear(); return false; }
    }
    public bool UpdatePrices(ProductCategory category, IReadOnlyList<PriceEditRow> rows)
    {
        using var transaction = db.Database.BeginTransaction();
        var products = db.Products.Where(p => p.Category == category).ToArray();
        if (rows.Count == 0 || products.Length != rows.Count || rows.Select(r => r.Id).Distinct().Count() != rows.Count) return false;
        var edits = rows.ToDictionary(r => r.Id);
        if (products.Any(p => !edits.TryGetValue(p.Id, out var row) || row.Price is null or < 0 or > 100000 || row.OriginalPrice != p.Price)) return false;
        foreach (var product in products) product.Price = edits[product.Id].Price!.Value;
        try { db.SaveChanges(); transaction.Commit(); return true; }
        catch (DbUpdateException) { db.ChangeTracker.Clear(); return false; }
    }
}
