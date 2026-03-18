using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Features.Orders.Commands.Create;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Tests.Orders;

public class CreateOrderCommandHandlerTests
{
    private static async Task<(AppDbContext db, UnitOfWork uow)> CreateDbAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);

        await db.Categories.AddAsync(new Category { Id = 1, Name = "Electronics" });
        await db.Products.AddRangeAsync(
            new Product { Id = 1, Name = "Laptop",  Description = "D", Price = 120m, StockQuantity = 10, CategoryId = 1 },
            new Product { Id = 2, Name = "Mouse",   Description = "D", Price = 30m,  StockQuantity = 5,  CategoryId = 1 },
            new Product { Id = 3, Name = "Keyboard",Description = "D", Price = 50m,  StockQuantity = 0,  CategoryId = 1 }
        );
        await db.SaveChangesAsync();

        var uow = new UnitOfWork(db);
        return (db, uow);
    }

    private static FakeNotificationService FakeNotif() => new();

    // ── Happy path ────────────────────────────────────────────────

    [Fact]
    public async Task Handle_SingleItem_AboveThreshold_AppliesDiscount()
    {
        var (_, uow) = await CreateDbAsync();
        var handler = new CreateOrderCommandHandler(uow, FakeNotif(), Microsoft.Extensions.Options.Options.Create(new ECommerce.Application.Options.OrderDiscountOptions()), new FakeScopeFactory());

        var result = await handler.Handle(
            new CreateOrderCommand(1, new CreateOrderRequest
            {
                Items = [new() { ProductId = 1, Quantity = 1 }]
            }), default);

        result.DiscountApplied.Should().Be(12m);    // 10% of 120
        result.TotalAmount.Should().Be(108m);        // 120 - 12
        result.Items.Should().HaveCount(1);
        result.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Handle_MultipleItems_CalculatesTotalCorrectly()
    {
        var (_, uow) = await CreateDbAsync();
        var handler = new CreateOrderCommandHandler(uow, FakeNotif(), Microsoft.Extensions.Options.Options.Create(new ECommerce.Application.Options.OrderDiscountOptions()), new FakeScopeFactory());

        // 120 + 30 = 150 → discount = 15 → total = 135
        var result = await handler.Handle(
            new CreateOrderCommand(1, new CreateOrderRequest
            {
                Items =
                [
                    new() { ProductId = 1, Quantity = 1 },
                    new() { ProductId = 2, Quantity = 1 }
                ]
            }), default);

        result.DiscountApplied.Should().Be(15m);
        result.TotalAmount.Should().Be(135m);
    }

    [Fact]
    public async Task Handle_SubtotalBelowThreshold_NoDiscount()
    {
        var (_, uow) = await CreateDbAsync();
        var handler = new CreateOrderCommandHandler(uow, FakeNotif(), Microsoft.Extensions.Options.Options.Create(new ECommerce.Application.Options.OrderDiscountOptions()), new FakeScopeFactory());

        // 30 < 100 → no discount
        var result = await handler.Handle(
            new CreateOrderCommand(1, new CreateOrderRequest
            {
                Items = [new() { ProductId = 2, Quantity = 1 }]
            }), default);

        result.DiscountApplied.Should().Be(0m);
        result.TotalAmount.Should().Be(30m);
    }

    [Fact]
    public async Task Handle_DeductsStockAfterOrder()
    {
        var (db, uow) = await CreateDbAsync();
        var handler = new CreateOrderCommandHandler(uow, FakeNotif(), Microsoft.Extensions.Options.Options.Create(new ECommerce.Application.Options.OrderDiscountOptions()), new FakeScopeFactory());

        await handler.Handle(
            new CreateOrderCommand(1, new CreateOrderRequest
            {
                Items = [new() { ProductId = 1, Quantity = 3 }]
            }), default);

        var product = await db.Products.FindAsync(1);
        product!.StockQuantity.Should().Be(7); // 10 - 3
    }

    [Fact]
    public async Task Handle_PersistsOrderItemsToDatabase()
    {
        var (db, uow) = await CreateDbAsync();
        var handler = new CreateOrderCommandHandler(uow, FakeNotif(), Microsoft.Extensions.Options.Options.Create(new ECommerce.Application.Options.OrderDiscountOptions()), new FakeScopeFactory());

        var result = await handler.Handle(
            new CreateOrderCommand(1, new CreateOrderRequest
            {
                Items = [new() { ProductId = 1, Quantity = 2 }]
            }), default);

        var items = db.OrderItems.Where(oi => oi.OrderId == result.Id).ToList();
        items.Should().HaveCount(1);
        items[0].Quantity.Should().Be(2);
        items[0].PriceAtPurchase.Should().Be(120m);
    }

    // ── Failure paths ─────────────────────────────────────────────

    [Fact]
    public async Task Handle_EmptyItemsList_ThrowsInvalidOperation()
    {
        var (_, uow) = await CreateDbAsync();
        var handler = new CreateOrderCommandHandler(uow, FakeNotif(), Microsoft.Extensions.Options.Options.Create(new ECommerce.Application.Options.OrderDiscountOptions()), new FakeScopeFactory());

        var act = () => handler.Handle(
            new CreateOrderCommand(1, new CreateOrderRequest { Items = [] }), default);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*at least one item*");
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsInvalidOperation()
    {
        var (_, uow) = await CreateDbAsync();
        var handler = new CreateOrderCommandHandler(uow, FakeNotif(), Microsoft.Extensions.Options.Options.Create(new ECommerce.Application.Options.OrderDiscountOptions()), new FakeScopeFactory());

        var act = () => handler.Handle(
            new CreateOrderCommand(1, new CreateOrderRequest
            {
                Items = [new() { ProductId = 999, Quantity = 1 }]
            }), default);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_InsufficientStock_ThrowsInvalidOperation()
    {
        var (_, uow) = await CreateDbAsync();
        var handler = new CreateOrderCommandHandler(uow, FakeNotif(), Microsoft.Extensions.Options.Options.Create(new ECommerce.Application.Options.OrderDiscountOptions()), new FakeScopeFactory());

        // Product 3 has StockQuantity = 0
        var act = () => handler.Handle(
            new CreateOrderCommand(1, new CreateOrderRequest
            {
                Items = [new() { ProductId = 3, Quantity = 1 }]
            }), default);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Insufficient stock*");
    }

    [Fact]
    public async Task Handle_ZeroQuantity_ThrowsInvalidOperation()
    {
        var (_, uow) = await CreateDbAsync();
        var handler = new CreateOrderCommandHandler(uow, FakeNotif(), Microsoft.Extensions.Options.Options.Create(new ECommerce.Application.Options.OrderDiscountOptions()), new FakeScopeFactory());

        var act = () => handler.Handle(
            new CreateOrderCommand(1, new CreateOrderRequest
            {
                Items = [new() { ProductId = 1, Quantity = 0 }]
            }), default);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*positive*");
    }

    // ── Stub ──────────────────────────────────────────────────────

    private class FakeNotificationService : ECommerce.Application.Interfaces.Services.INotificationService
    {
        public Task SendAsync(ECommerce.Application.DTOs.Notification.NotificationDto dto, CancellationToken ct = default) => Task.CompletedTask;
        public Task SendToUserAsync(string userId, ECommerce.Application.DTOs.Notification.NotificationDto dto, CancellationToken ct = default) => Task.CompletedTask;
        public Task BroadcastStockUpdateAsync(int productId, int newStockQuantity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private class FakeScopeFactory : Microsoft.Extensions.DependencyInjection.IServiceScopeFactory
    {
        public Microsoft.Extensions.DependencyInjection.IServiceScope CreateScope() => new FakeScope();

        private sealed class FakeScope : Microsoft.Extensions.DependencyInjection.IServiceScope
        {
            public IServiceProvider ServiceProvider { get; } = new FakeServiceProvider();
            public void Dispose() { }
        }

        private sealed class FakeServiceProvider : IServiceProvider
        {
            public object? GetService(Type serviceType) => null;
        }
    }
}


