using OrderSystem.Models;

namespace OrderSystem.Services;

public class InMemoryProductService : IProductService
{
    private readonly List<Product> _products;
    private int _nextId;

    public InMemoryProductService()
    {
        _products = new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "AK-47 | Redline",
                Game = "Counter-Strike 2",
                Category = "Скіни",
                Price = 650,
                Description = "Скін AK-47 Redline. Передача через Steam Trade.",
                SellerName = "AlexTrade",
                IsAvailable = true
            },
            new Product
            {
                Id = 2,
                Name = "1000 Gold",
                Game = "World of Warcraft",
                Category = "Ігрова валюта",
                Price = 300,
                Description = "1000 одиниць золота.",
                SellerName = "GoldMaster",
                IsAvailable = true
            },
            new Product
            {
                Id = 3,
                Name = "GTA V Account",
                Game = "GTA V",
                Category = "Акаунти",
                Price = 900,
                Description = "Акаунт GTA V з прокачаним персонажем.",
                SellerName = "GameSeller",
                IsAvailable = true
            }
        };

        _nextId = 4;
    }

    // Singleton storage is shared by concurrent requests. Never expose stored instances.
    private readonly object _gate = new();

    public IEnumerable<Product> GetAll()
    {
        lock (_gate) return _products.Select(Copy).ToArray();
    }

    public Product? GetById(int id)
    {
        lock (_gate)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            return product is null ? null : Copy(product);
        }
    }

    public void Add(Product product)
    {
        lock (_gate)
        {
            var stored = Copy(product);
            stored.Id = _nextId++;
            _products.Add(stored);
            product.Id = stored.Id;
        }
    }

    public bool Update(Product product)
    {
        lock (_gate)
        {
            var index = _products.FindIndex(p => p.Id == product.Id);
            if (index < 0) return false;
            _products[index] = Copy(product);
            return true;
        }
    }

    public bool Delete(int id)
    {
        lock (_gate) return _products.RemoveAll(p => p.Id == id) > 0;
    }

    private static Product Copy(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Game = product.Game,
        Category = product.Category,
        Price = product.Price,
        Description = product.Description,
        SellerName = product.SellerName,
        IsAvailable = product.IsAvailable
    };
}
