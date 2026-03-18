using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Features.Products.Queries.All;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.Tests.Products;

public class GetProductsQueryTests
{
    private static IMemoryCache GetMemoryCache() => new MemoryCache(new MemoryCacheOptions());

    private static GetProductsPagedQueryHandler CreateHandler(UnitOfWork uow) => new(uow, GetMemoryCache());

    private static async Task<(AppDbContext, UnitOfWork)> SeedAsync()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var db = new AppDbContext(opts);

        var cat1 = new Category { Id = 1, Name = "Electronics" };
        var cat2 = new Category { Id = 2, Name = "Books" };
        await db.Categories.AddRangeAsync(cat1, cat2);
        await db.Products.AddRangeAsync(
            new Product { Name = "Laptop",  Description = "Powerful laptop", Price = 999m, StockQuantity = 5, CategoryId = 1 },
            new Product { Name = "Phone",   Description = "Smartphone",      Price = 599m, StockQuantity = 10, CategoryId = 1 },
            new Product { Name = "Novel",   Description = "Great novel",     Price = 15m,  StockQuantity = 20, CategoryId = 2 },
            new Product { Name = "Textbook",Description = "Study textbook",  Price = 45m,  StockQuantity = 8,  CategoryId = 2 }
        );
        await db.SaveChangesAsync();
        return (db, new UnitOfWork(db));
    }

    [Fact]
    public async Task ReturnsAllProducts_WhenNoFilters()
    {
        var (_, uow) = await SeedAsync();
        var handler = CreateHandler(uow);

        var result = await handler.Handle(new GetProductsPagedQuery(1, 10, null, null, null, false), default);

        result.Items.Should().HaveCount(4);
        result.TotalCount.Should().Be(4);
    }

    [Fact]
    public async Task Pagination_ReturnsCorrectPage()
    {
        var (_, uow) = await SeedAsync();
        var handler = CreateHandler(uow);

        var result = await handler.Handle(new GetProductsPagedQuery(2, 2, null, null, null, false), default);

        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(2);
        result.TotalCount.Should().Be(4);
    }

    [Fact]
    public async Task Search_FiltersProductsByName()
    {
        var (_, uow) = await SeedAsync();
        var handler = CreateHandler(uow);

        var result = await handler.Handle(new GetProductsPagedQuery(1, 10, "laptop", null, null, false), default);

        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task Search_FiltersProductsByDescription()
    {
        var (_, uow) = await SeedAsync();
        var handler = CreateHandler(uow);

        var result = await handler.Handle(new GetProductsPagedQuery(1, 10, "smartphone", null, null, false), default);

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task FilterByCategory_ReturnsOnlyMatchingProducts()
    {
        var (_, uow) = await SeedAsync();
        var handler = CreateHandler(uow);

        var result = await handler.Handle(new GetProductsPagedQuery(1, 10, null, 2, null, false), default);

        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(p => p.CategoryName.Should().Be("Books"));
    }

    [Fact]
    public async Task SortByPrice_Ascending_ReturnsCorrectOrder()
    {
        var (_, uow) = await SeedAsync();
        var handler = CreateHandler(uow);

        var result = await handler.Handle(new GetProductsPagedQuery(1, 10, null, null, "price", false), default);

        result.Items.Select(p => p.Price).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task SortByPrice_Descending_ReturnsCorrectOrder()
    {
        var (_, uow) = await SeedAsync();
        var handler = CreateHandler(uow);

        var result = await handler.Handle(new GetProductsPagedQuery(1, 10, null, null, "price", true), default);

        result.Items.Select(p => p.Price).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task EmptyResult_WhenSearchMatchesNothing()
    {
        var (_, uow) = await SeedAsync();
        var handler = CreateHandler(uow);

        var result = await handler.Handle(new GetProductsPagedQuery(1, 10, "xyznonexistent", null, null, false), default);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}


