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
        if (ProductCategories.TryParse(filter.Category, out var selectedCategory))
            filter.Category = ((ProductCategory?)selectedCategory).Label();
        var all = _productService.GetAll().ToArray();
        IEnumerable<Product> products = all;
        if (ModelState.IsValid)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
                products = products.Where(p => p.Name.Contains(filter.Search.Trim(), StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(filter.Game))
                products = products.Where(p => p.Game.Equals(filter.Game, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(filter.Category))
                products = products.Where(p => ProductCategories.TryParse(filter.Category, out var category) && p.Category == category);
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
            Categories = Enum.GetValues<ProductCategory>().Select(c => ((ProductCategory?)c).Label()).ToArray()
        });
    }

    [HttpGet]
    public IActionResult EditPrices([FromQuery] ProductCategory? category)
    {
        if (!ModelState.IsValid || !category.HasValue || !Enum.IsDefined(category.Value)) return BadRequest();
        return View(new EditPricesViewModel
        {
            Category = category,
            Rows = _productService.GetAll().Where(p => p.Category == category).Select(p => new PriceEditRow
            {
                Id = p.Id, Name = p.Name, Price = p.Price, OriginalPrice = p.Price
            }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditPrices([FromForm] EditPricesViewModel model)
    {
        if (!model.Category.HasValue || !Enum.IsDefined(model.Category.Value)) return BadRequest();
        var current = _productService.GetAll().Where(p => p.Category == model.Category).ToDictionary(p => p.Id);
        // Names are display-only and always loaded from storage, not from the submitted form.
        foreach (var row in model.Rows) row.Name = current.GetValueOrDefault(row.Id)?.Name ?? "Оголошення недоступне";
        if (ModelState.IsValid && !_productService.UpdatePrices(model.Category.Value, model.Rows))
            ModelState.AddModelError("", "Список або ціни оголошень змінилися. Нічого не збережено. Оновіть сторінку й повторіть спробу.");
        if (!ModelState.IsValid) return View(model);
        TempData["CatalogMessage"] = "Усі ціни категорії збережено.";
        return RedirectToAction(nameof(Index), new { Category = model.Category.Value.ToString() });
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

        return RedirectToAction(nameof(Details), new { id = product.Id });
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

        return RedirectToAction(nameof(Details), new { id = product.Id });
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