using OrderSystem.Middleware;
using Microsoft.EntityFrameworkCore;
using OrderSystem.Data;
using OrderSystem.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IMarketplaceInfoService, MarketplaceInfoService>();
var connectionString = builder.Configuration.GetConnectionString("Marketplace") ?? "Data Source=App_Data/marketplace.db";
Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "App_Data"));
builder.Services.AddDbContext<MarketplaceContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<IProductService, EfProductService>();
builder.Services.AddScoped<IUserDirectoryService, EfUserDirectoryService>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<MarketplaceContext>();
    database.Database.Migrate();
    SampleData.Initialize(database);
}

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