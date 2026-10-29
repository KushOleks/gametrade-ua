using Microsoft.AspNetCore.Mvc;
using OrderSystem.Models;
using OrderSystem.Services;

namespace OrderSystem.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public IActionResult Index([FromQuery] ProductFilter filter)
    {
        var all = _productService.GetAll().ToArray();
        IEnumerable<Product> products = all;
        if (ModelState.IsValid)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
                products = products.Where(p => p.Name.Contains(filter.Search.Trim(), StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(filter.Game))
                products = products.Where(p => p.Game.Equals(filter.Game, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(filter.Category))
                products = products.Where(p => p.Category.Equals(filter.Category, StringComparison.OrdinalIgnoreCase));
            if (filter.MinPrice.HasValue) products = products.Where(p => p.Price >= filter.MinPrice.Value);
            if (filter.MaxPrice.HasValue) products = products.Where(p => p.Price <= filter.MaxPrice.Value);
            if (filter.OnlyAvailable) products = products.Where(p => p.IsAvailable);
        }
        else
        {
            products = Array.Empty<Product>();
        }

        return View(new ProductCatalogViewModel
        {
            Filter = filter,
            Products = products.ToArray(),
            Games = all.Select(p => p.Game).Distinct().OrderBy(g => g).ToArray(),
            Categories = all.Select(p => p.Category).Distinct().OrderBy(c => c).ToArray()
        });
    }

    private IActionResult ProductNotFound()
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        return View("ProductNotFound");
    }

    public IActionResult Details(int id)
    {
        var product = _productService.GetById(id);

        if (product == null)
        {
            return ProductNotFound();
        }

        return View(product);
    }

    public IActionResult Create()
    {
        return View(new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("Name,Game,Category,Price,Description,SellerName,IsAvailable")] Product product)
    {
        if (!ModelState.IsValid)
        {
            return View(product);
        }

        _productService.Add(product);

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var product = _productService.GetById(id);

        if (product == null)
        {
            return ProductNotFound();
        }

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit([FromRoute] int id, [Bind("Id,Name,Game,Category,Price,Description,SellerName,IsAvailable")] Product product)
    {
        if (id != product.Id) return BadRequest();
        if (_productService.GetById(id) is null) return ProductNotFound();

        if (!ModelState.IsValid)
        {
            return View(product);
        }

        if (!_productService.Update(product))
        {
            return ProductNotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var product = _productService.GetById(id);

        if (product == null)
        {
            return ProductNotFound();
        }

        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        if (!_productService.Delete(id))
        {
            return ProductNotFound();
        }

        return RedirectToAction(nameof(Index));
    }
}