using OrderSystem.Middleware;
using OrderSystem.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IMarketplaceInfoService, MarketplaceInfoService>();
builder.Services.AddSingleton<IProductService, InMemoryProductService>();
builder.Services.AddSingleton<IUserDirectoryService, DemoUserDirectoryService>();

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Explicit aliases reuse the existing actions and views.
app.MapControllerRoute("product-details", "products/{id:int}", new { controller = "Products", action = "Details" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute("items", "items/{action=Index}/{id?}", new { controller = "Products" });
app.MapControllerRoute("product-new", "products/new", new { controller = "Products", action = "Create" });

app.Run();