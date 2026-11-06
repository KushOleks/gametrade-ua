using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderSystem.Data;
using OrderSystem.Models;
namespace OrderSystem.Controllers;
public class CustomersController(MarketplaceContext db) : Controller
{
    public IActionResult Index(string? search, string sort = "name", int page = 1)
    {
        var query = db.Customers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(c => c.FirstName.Contains(search) || c.LastName.Contains(search));
        query = sort switch
        {
            "name_desc" => query.OrderByDescending(c => c.LastName).ThenBy(c => c.Id),
            "city" => query.OrderBy(c => c.City).ThenBy(c => c.Id),
            "city_desc" => query.OrderByDescending(c => c.City).ThenBy(c => c.Id),
            _ => query.OrderBy(c => c.LastName).ThenBy(c => c.Id)
        };
        const int pageSize = 5;
        var pages = Math.Max(1, (int)Math.Ceiling(query.Count() / (double)pageSize));
        page = Math.Clamp(page, 1, pages);
        return View(new DirectoryViewModel<Customer> { Items = query.Skip((page - 1) * pageSize).Take(pageSize).ToArray(), Search = search, Sort = sort, Page = page, Pages = pages });
    }
    public IActionResult Details(int id) => db.Customers.AsNoTracking().SingleOrDefault(c => c.Id == id) is { } item ? View(item) : NotFound();
    public IActionResult Create() => View(new Customer());
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create([Bind("FirstName,LastName,City")] Customer item)
    {
        if (!ModelState.IsValid) return View(item);
        db.Customers.Add(item);
        try { db.SaveChanges(); }
        catch (DbUpdateException) { ModelState.AddModelError("", "Не вдалося зберегти. Перевірте унікальність назви та повторіть спробу."); return View(item); }
        return RedirectToAction(nameof(Details), new { id = item.Id });
    }
    public IActionResult Edit(int id) => db.Customers.AsNoTracking().SingleOrDefault(c => c.Id == id) is { } item ? View(item) : NotFound();
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit([FromRoute] int id, [Bind("Id,FirstName,LastName,City")] Customer item)
    {
        if (id != item.Id) return BadRequest();
        if (!db.Customers.Any(c => c.Id == id)) return NotFound();
        if (!ModelState.IsValid) return View(item);
        db.Customers.Update(item);
        try { db.SaveChanges(); }
        catch (DbUpdateException) { ModelState.AddModelError("", "Не вдалося зберегти. Перевірте дані та повторіть спробу."); return View(item); }
        return RedirectToAction(nameof(Details), new { id });
    }
    public IActionResult Delete(int id) => db.Customers.AsNoTracking().SingleOrDefault(c => c.Id == id) is { } item ? View(item) : NotFound();
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var item = db.Customers.Find(id);
        if (item is null) return NotFound();
        if (db.Orders.Any(o => o.CustomerId == id)) { ModelState.AddModelError("", "Запис використовується в товарах або замовленнях. Спочатку видаліть залежні записи."); return View("Delete", item); }
        db.Customers.Remove(item);
        try { db.SaveChanges(); }
        catch (DbUpdateException) { ModelState.AddModelError("", "Запис використовується в інших даних і не може бути видалений."); return View("Delete", item); }
        return RedirectToAction(nameof(Index));
    }
}
