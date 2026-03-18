using System;
using System.Linq;
using System.Transactions;
using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.DTOs.Notification;
using ECommerce.Application.Options;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ECommerce.Application.Features.Orders.Commands.Create;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;
    private readonly OrderDiscountOptions _discountOptions;
    private readonly IServiceScopeFactory _scopeFactory;

    public CreateOrderCommandHandler(
        IUnitOfWork uow,
        INotificationService notificationService,
        IOptions<OrderDiscountOptions> discountOptions,
        IServiceScopeFactory scopeFactory)
    {
        _uow = uow;
        _notificationService = notificationService;
        _discountOptions = discountOptions?.Value ?? new OrderDiscountOptions();
        _scopeFactory = scopeFactory;
    }

    public async Task<OrderResponseDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Request.Items == null || !request.Request.Items.Any())
            throw new InvalidOperationException("Order must have at least one item.");

        var productRepo  = _uow.Repository<Product>();
        var orderRepo    = _uow.Repository<Order>();
        var orderItemRepo = _uow.Repository<OrderItem>();

        var productIds = request.Request.Items.Select(i => i.ProductId).Distinct().ToList();
        var prodQuery = productRepo.Query().Where(p => productIds.Contains(p.Id) && !p.IsDeleted);
        var products = await productRepo.ToListAsync(prodQuery, cancellationToken);

        if (products.Count != productIds.Count)
            throw new InvalidOperationException("One or more products not found.");

        // Collect all stock errors so the user sees every unavailable item in one message
        var stockErrors = new List<string>();
        foreach (var item in request.Request.Items)
        {
            var product = products.Single(p => p.Id == item.ProductId);
            if (item.Quantity <= 0)
            {
                stockErrors.Add($"'{product.Name}': quantity must be at least 1.");
                continue;
            }
            if (product.StockQuantity < item.Quantity)
            {
                if (product.StockQuantity == 0)
                    stockErrors.Add($"'{product.Name}': out of stock (you requested {item.Quantity}).");
                else
                    stockErrors.Add($"'{product.Name}': only {product.StockQuantity} in stock (you requested {item.Quantity}).");
            }
        }
        if (stockErrors.Count > 0)
        {
            var message = "Cannot place order. Some items are no longer available in the quantity you requested. Please update your cart. " + string.Join(" ", stockErrors);
            throw new InvalidOperationException(message);
        }

        decimal subtotal = 0m;
        decimal perProductDiscount = 0m;

        // Compute subtotal (with optional per-product discount)
        foreach (var item in request.Request.Items)
        {
            var product = products.Single(p => p.Id == item.ProductId);
            var lineTotal = product.Price * item.Quantity;
            if (product.DiscountRate > 0 && product.DiscountRate <= 100)
                perProductDiscount += Math.Round(lineTotal * (product.DiscountRate / 100m), 2);
            subtotal += lineTotal;
        }

        decimal totalDiscount = perProductDiscount;
        decimal total = Math.Round(subtotal - totalDiscount, 2);
        string? discountDescription = totalDiscount > 0 ? "Product discount" : null;

        var order = new Order
        {
            UserId          = request.UserId,
            TotalAmount     = total,
            DiscountApplied = totalDiscount,
            OrderDate       = DateTime.UtcNow,
            Status          = OrderStatus.Pending
        };

        if (request.Request.Address != null)
        {
            order.ShippingStreet  = request.Request.Address.Street;
            order.ShippingCity    = request.Request.Address.City;
            order.ShippingCountry = request.Request.Address.Country;
        }

        await orderRepo.AddAsync(order, cancellationToken);

        var responseItems = new List<OrderItemResponseDto>();

        // Single transaction: order + items + stock deduction only. Notification is saved after scope is disposed.
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                foreach (var item in request.Request.Items)
                {
                    var product = products.Single(p => p.Id == item.ProductId);

                    product.StockQuantity -= item.Quantity;
                    productRepo.Update(product);

                    var orderItem = new OrderItem
                    {
                        Order           = order,
                        ProductId       = product.Id,
                        Quantity        = item.Quantity,
                        PriceAtPurchase = product.Price,
                        DiscountAtPurchase = product.DiscountRate
                    };
                    await orderItemRepo.AddAsync(orderItem, cancellationToken);

                    responseItems.Add(new OrderItemResponseDto
                    {
                        ProductId       = product.Id,
                        ProductName     = product.Name,
                        Quantity        = item.Quantity,
                        PriceAtPurchase = product.Price,
                        DiscountAtPurchase = product.DiscountRate
                    });
                }

                await _uow.SaveChangesAsync(cancellationToken);
                scope.Complete();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException("Stock was updated by another order. Please refresh and try again.");
            }
        }

        // After scope is disposed, no ambient transaction — safe to use DbContext again for notification
        foreach (var item in request.Request.Items)
        {
            var product = products.Single(p => p.Id == item.ProductId);
            await _notificationService.BroadcastStockUpdateAsync(product.Id, product.StockQuantity, cancellationToken);
        }

        var estimatedDelivery = order.OrderDate.AddDays(7);

        var notification = new Notification
        {
            UserId    = request.UserId,
            OrderId   = order.Id,
            Type      = "Success",
            Message   = $"Great news! Your order #{order.Id} has been successfully placed. Total: {total:C}.",
            IsRead    = false,
            CreatedAt = DateTime.UtcNow
        };
        await _uow.Repository<Notification>().AddAsync(notification, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _notificationService.SendToUserAsync(request.UserId.ToString(), new NotificationDto
        {
            Id        = notification.Id,
            UserId    = notification.UserId ?? 0,
            OrderId   = order.Id,
            Message   = notification.Message,
            Type      = notification.Type,
            IsRead    = notification.IsRead,
            CreatedAt = notification.CreatedAt
        }, cancellationToken);

        // Delayed "Shipped" notification (new scope to avoid disposed scoped services)
        var orderId = order.Id;
        var userIdStr = request.UserId.ToString();
        var estDeliveryStr = estimatedDelivery.ToString("MMM d, yyyy");
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var notifSvc = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var notification = new Notification
                {
                    UserId    = request.UserId,
                    OrderId   = orderId,
                    Type      = "Info",
                    Message   = $"We're getting your order #{orderId} ready! Estimated delivery is {estDeliveryStr}.",
                    IsRead    = false,
                    CreatedAt = DateTime.UtcNow
                };
                await uow.Repository<Notification>().AddAsync(notification, default);
                await uow.SaveChangesAsync(default);
                await notifSvc.SendToUserAsync(userIdStr, new NotificationDto
                {
                    Id = notification.Id,
                    UserId = notification.UserId ?? 0,
                    OrderId = orderId,
                    Message = notification.Message,
                    Type = notification.Type,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt
                }, default);
            }
            catch
            {
                // Best-effort; do not fail order
            }
        }, cancellationToken);

        return new OrderResponseDto
        {
            Id                    = order.Id,
            Subtotal              = subtotal,
            TotalAmount           = order.TotalAmount,
            DiscountApplied       = order.DiscountApplied,
            DiscountDescription   = discountDescription,
            ItemsCount            = responseItems.Count,
            ItemsQuantity         = responseItems.Sum(i => i.Quantity),
            OrderDate             = order.OrderDate,
            EstimatedDeliveryDate = estimatedDelivery,
            Status                = order.Status.ToString(),
            Address               = request.Request.Address,
            Items                 = responseItems
        };
    }
}
