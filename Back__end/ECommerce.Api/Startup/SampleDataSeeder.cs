using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json;

namespace ECommerce.Api.Startup;

/// <summary>
/// Provides sample data seeding functionality for development and testing environments.
/// </summary>
public static class SampleDataSeeder
{
    // helper types for deserializing the JSON file
    private class JsonCategory
    {
        public string id { get; set; } = default!;
        public string name { get; set; } = default!;
        public string icon { get; set; } = default!;
    }

    private class JsonProduct
    {
        public string id { get; set; } = default!;
        public string name { get; set; } = default!;
        public decimal price { get; set; }
        public int quantity { get; set; }
        public string imageUrl { get; set; } = default!;
        public string categoryID { get; set; } = default!;
        public string description { get; set; } = default!;
    }

    private class DbJson
    {
        public List<JsonProduct> products { get; set; } = new();
        public List<JsonCategory> categories { get; set; } = new();
    }

  /// <summary>
  /// Seeds the database with sample data asynchronously.
  /// </summary>
  /// <param name="services">The service provider containing database context and other services.</param>
  /// <returns>A task representing the asynchronous operation.</returns>
  public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // only seed once
        if (await context.Categories.AnyAsync() || await context.Products.AnyAsync())
            return;

        // try reading a JSON file shipped with the API project
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Startup", "seeddata.json");
        if (File.Exists(jsonPath))
        {
            var jsonText = await File.ReadAllTextAsync(jsonPath);
            var dbData = JsonSerializer.Deserialize<DbJson>(jsonText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (dbData != null)
            {
                Console.WriteLine("=== SEEDING FROM JSON ===");

                // create categories from JSON
                var categories = dbData.categories
                    .Select(c => new Category { Name = c.name })
                    .ToList();
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();

                // build a lookup from JSON category id -> name
                var idToName = dbData.categories
                    .ToDictionary(c => int.Parse(c.id), c => c.name);

                // map products to entities
                var products = dbData.products.Select(p =>
                {
                    var catName = idToName[int.Parse(p.categoryID)];
                    var categoryEntity = categories.First(c => c.Name == catName);
                    return new Product
                    {
                        Name = p.name,
                        Description = p.description,
                        Price = p.price,
                        StockQuantity = p.quantity,
                        Category = categoryEntity,
                        ImageUrl = string.IsNullOrWhiteSpace(p.imageUrl)
                            ? $"https://picsum.photos/seed/{p.id}/400/300"
                            : p.imageUrl
                    };
                }).ToList();

                context.Products.AddRange(products);
                await context.SaveChangesAsync();

                Console.WriteLine($"Seeded {products.Count} products");
                return;
            }
        }

        // fallback to original random seeding if JSON isn't available or fails to deserialize
        var categoryNames = new[]
        {
            "Electronics", "Clothing", "Books", "Home", "Toys",
            "Groceries", "Sports", "Beauty", "Automotive", "Garden"
        };

        var randomCategories = categoryNames.Select(n => new Category { Name = n }).ToList();
        context.Categories.AddRange(randomCategories);

        var productsList = new List<Product>();
        var rnd = new Random(123);
        foreach (var cat in randomCategories)
        {
            for (int i = 1; i <= 6; i++)
            {
                productsList.Add(new Product
                {
                    Name = $"{cat.Name} Item {i}",
                    Description = $"Sample {cat.Name.ToLower()} product number {i}.",
                    Price = Math.Round((decimal)(rnd.NextDouble() * 100 + 5), 2),
                    StockQuantity = rnd.Next(0, 100),
                    Category = cat,
                    ImageUrl = $"https://picsum.photos/seed/{cat.Name}-{i}/400/300"
                });
            }
        }

        context.Products.AddRange(productsList);
        await context.SaveChangesAsync();
    }
}