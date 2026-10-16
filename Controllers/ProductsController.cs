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

    public IActionResult Index()
    {
        var products = _productService.GetAll();
        return View(products);
    }

    public IActionResult Details(int id)
    {
        var product = _productService.GetById(id);

        if (product == null)
        {
            return NotFound();
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
            return NotFound();
        }

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit([FromRoute] int id, [Bind("Id,Name,Game,Category,Price,Description,SellerName,IsAvailable")] Product product)
    {
        if (id != product.Id) return BadRequest();
        if (_productService.GetById(id) is null) return NotFound();

        if (!ModelState.IsValid)
        {
            return View(product);
        }

        if (!_productService.Update(product))
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var product = _productService.GetById(id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        if (!_productService.Delete(id))
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }
}