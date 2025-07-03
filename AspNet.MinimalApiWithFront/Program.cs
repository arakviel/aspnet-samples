using AspNet.MinimalApiWithFront.Data;
using AspNet.MinimalApiWithFront.Models;
using AspNet.MinimalApiWithFront.Repositories;
using AspNet.MinimalApiWithFront.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Додавання сервісів до контейнера
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

// Додавання CORS для фронтенду
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Створення бази даних та застосування міграцій
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

// Конфігурація HTTP pipeline
app.UseCors();

// Налаштування MIME типів для JavaScript модулів
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".js"] = "application/javascript";
provider.Mappings[".mjs"] = "application/javascript";
provider.Mappings[".ts"] = "application/javascript";

// Налаштування статичних файлів для скомпільованого TypeScript
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "dist")),
    RequestPath = "",
    ContentTypeProvider = provider
});

// Резервні статичні файли з wwwroot (для розробки)
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

// API endpoints для товарів

/// <summary>
/// Отримує всі товари
/// </summary>
app.MapGet("/api/products", async (IProductService productService) =>
{
    var products = await productService.GetAllProductsAsync();
    return products;
})
.WithName("GetProducts");

/// <summary>
/// Отримує товар за ідентифікатором
/// </summary>
app.MapGet("/api/products/{id:int}", async (int id, IProductService productService) =>
{
    var product = await productService.GetProductByIdAsync(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
})
.WithName("GetProduct");

/// <summary>
/// Створює новий товар
/// </summary>
app.MapPost("/api/products", async (Product product, IProductService productService) =>
{
    try
    {
        var createdProduct = await productService.CreateProductAsync(product);
        return Results.Created($"/api/products/{createdProduct.Id}", createdProduct);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CreateProduct");

/// <summary>
/// Оновлює існуючий товар
/// </summary>
app.MapPut("/api/products/{id:int}", async (int id, Product product, IProductService productService) =>
{
    try
    {
        var updatedProduct = await productService.UpdateProductAsync(id, product);
        return updatedProduct is not null ? Results.Ok(updatedProduct) : Results.NotFound();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("UpdateProduct");

/// <summary>
/// Видаляє товар
/// </summary>
app.MapDelete("/api/products/{id:int}", async (int id, IProductService productService) =>
{
    var deleted = await productService.DeleteProductAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteProduct");

// Головна сторінка
app.MapGet("/", () => Results.Redirect("/index.html"));

app.Run();