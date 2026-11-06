using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderSystem.Data;
using OrderSystem.Models;
namespace OrderSystem.Controllers;

public class OrdersController(MarketplaceContext db) : Controller
{
    private IQueryable<Order> CompleteOrders() => db.Orders.Include(o => o.Customer).Include(o => o.Seller).Include(o => o.Lines);
    public IActionResult Index() => View(CompleteOrders().AsNoTracking().OrderByDescending(o => o.CreatedAtUtc).ThenByDescending(o => o.Id).ToArray());
    public IActionResult Details(int id) => CompleteOrders().AsNoTracking().SingleOrDefault(o => o.Id == id) is { } order ? View(order) : NotFound();

    private void FillChoices(OrderFormViewModel model)
    {
        model.Customers = db.Customers.AsNoTracking().OrderBy(c => c.LastName).ToArray();
        model.Sellers = db.Sellers.AsNoTracking().OrderBy(s => s.Name).ToArray();
    }
    public IActionResult Create(int sellerId = 0)
    {
        var model = new OrderFormViewModel { SellerId = sellerId };
        FillChoices(model);
        model.Rows = db.Products.AsNoTracking().Where(p => p.SellerId == sellerId && p.IsAvailable).OrderBy(p => p.Id)
            .Select(p => new OrderQuantityRow { ProductId = p.Id, Name = p.Name, UnitPrice = p.Price }).ToList();
        return View(model);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create([FromForm] OrderFormViewModel model)
    {
        using var transaction = db.Database.BeginTransaction();
        FillChoices(model);
        if (!db.Customers.Any(c => c.Id == model.CustomerId)) ModelState.AddModelError("CustomerId", "Покупця не знайдено");
        if (!db.Sellers.Any(s => s.Id == model.SellerId)) ModelState.AddModelError("SellerId", "Продавця не знайдено");
        var products = db.Products.AsNoTracking().Where(p => p.SellerId == model.SellerId && p.IsAvailable).ToDictionary(p => p.Id);
        if (model.Rows.Count == 0 || !model.Rows.Any(r => r.Quantity > 0)) ModelState.AddModelError("", "Оберіть хоча б один товар: вкажіть кількість більшу за 0");
        if (model.Rows.Select(r => r.ProductId).Distinct().Count() != model.Rows.Count) ModelState.AddModelError("", "Товар не може повторюватися");
        foreach (var row in model.Rows)
        {
            if (!products.TryGetValue(row.ProductId, out var product))
            {
                row.Name = "Товар недоступний";
                ModelState.AddModelError("", "Товар недоступний або належить іншому продавцю. Оновіть форму.");
            }
            else { row.Name = product.Name; row.UnitPrice = product.Price; }
        }
        if (!ModelState.IsValid) return View(model);
        var order = new Order
        {
            CustomerId = model.CustomerId, SellerId = model.SellerId, CreatedAtUtc = DateTime.UtcNow,
            Lines = model.Rows.Where(r => r.Quantity > 0).Select(r => new OrderLine
            {
                ProductId = r.ProductId, ProductName = products[r.ProductId].Name,
                UnitPrice = products[r.ProductId].Price, Quantity = r.Quantity
            }).ToList()
        };
        db.Orders.Add(order);
        try { db.SaveChanges(); transaction.Commit(); }
        catch (DbUpdateException) { ModelState.AddModelError("", "Дані змінилися. Оновіть форму та повторіть спробу."); return View(model); }
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }
    public IActionResult Edit(int id)
    {
        var order = CompleteOrders().AsNoTracking().SingleOrDefault(o => o.Id == id);
        if (order is null) return NotFound();
        var model = new OrderFormViewModel
        {
            Id = id, Version = order.Version, CustomerId = order.CustomerId, SellerId = order.SellerId,
            Rows = order.Lines.OrderBy(l => l.Id).Select(l => new OrderQuantityRow { ProductId = l.ProductId, Name = l.ProductName, UnitPrice = l.UnitPrice, Quantity = l.Quantity }).ToList()
        };
        FillChoices(model);
        return View(model);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit([FromRoute] int id, [FromForm] OrderFormViewModel model)
    {
        if (id != model.Id) return BadRequest();
        var order = db.Orders.Include(o => o.Lines).SingleOrDefault(o => o.Id == id);
        if (order is null) return NotFound();
        FillChoices(model);
        if (model.Version != order.Version) ModelState.AddModelError("", "Замовлення змінилося. Оновіть сторінку.");
        if (model.SellerId != order.SellerId) ModelState.AddModelError("", "Не можна змінити продавця вже створеного замовлення");
        if (!db.Customers.Any(c => c.Id == model.CustomerId)) ModelState.AddModelError("CustomerId", "Покупця не знайдено");
        var lines = order.Lines.ToDictionary(l => l.ProductId);
        if (model.Rows.Count != lines.Count || model.Rows.Select(r => r.ProductId).Distinct().Count() != model.Rows.Count || model.Rows.Any(r => !lines.ContainsKey(r.ProductId)))
            ModelState.AddModelError("", "Склад замовлення змінився. Оновіть сторінку.");
        foreach (var row in model.Rows)
        {
            row.Name = lines.GetValueOrDefault(row.ProductId)?.ProductName ?? "Товар недоступний";
            row.UnitPrice = lines.GetValueOrDefault(row.ProductId)?.UnitPrice ?? 0;
        }
        if (!model.Rows.Any(r => r.Quantity > 0)) ModelState.AddModelError("", "Замовлення має містити хоча б один товар");
        if (!ModelState.IsValid) return View(model);
        foreach (var row in model.Rows)
        {
            var line = lines[row.ProductId];
            if (row.Quantity == 0) db.OrderLines.Remove(line);
            else line.Quantity = row.Quantity;
        }
        order.CustomerId = model.CustomerId;
        order.Version = Guid.NewGuid().ToString();
        try { db.SaveChanges(); }
        catch (DbUpdateException) { ModelState.AddModelError("", "Дані змінилися. Оновіть сторінку."); return View(model); }
        return RedirectToAction(nameof(Details), new { id });
    }
    public IActionResult Delete(int id) => CompleteOrders().AsNoTracking().SingleOrDefault(o => o.Id == id) is { } order ? View(order) : NotFound();
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id, string? version)
    {
        var order = CompleteOrders().SingleOrDefault(o => o.Id == id);
        if (order is null) return NotFound();
        if (version != order.Version) { ModelState.AddModelError("", "Замовлення змінилося. Оновіть сторінку перед видаленням."); return View("Delete", order); }
        db.Orders.Remove(order);
        try { db.SaveChanges(); }
        catch (DbUpdateException) { ModelState.AddModelError("", "Замовлення змінилося. Оновіть сторінку."); return View("Delete", order); }
        return RedirectToAction(nameof(Index));
    }
}
