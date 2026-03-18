using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Domain.Entities;
using MediatR;

// avoiding EF Core imports here; repository handles async ops

namespace ECommerce.Application.Features.Orders.Queries.ById;

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponseDto?>
{
    private readonly IUnitOfWork _uow;

    public GetOrderByIdQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<OrderResponseDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _uow.Repository<Order>().Query();
        var included = Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include(query, (Order o) => o.Items);
        query = Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ThenInclude<Order, OrderItem, Product>(included, i => i.Product);
        query = query.Where(o => o.Id == request.OrderId && o.UserId == request.UserId);

        var orders = await _uow.Repository<Order>().ToListAsync(query, cancellationToken);
        var order = orders.FirstOrDefault();

        if (order is null)
        {
            return null;
        }

        var subtotal = order.Items.Sum(i => i.PriceAtPurchase * i.Quantity);
        var estimatedDelivery = order.OrderDate.AddDays(7);

        return new OrderResponseDto
        {
            Id = order.Id,
            Subtotal = subtotal,
            TotalAmount = order.TotalAmount,
            DiscountApplied = order.DiscountApplied,
            DiscountDescription = order.DiscountApplied > 0 ? "Product discount" : null,
            ItemsCount = order.Items.Count,
            ItemsQuantity = order.Items.Sum(i => i.Quantity),
            OrderDate = order.OrderDate,
            EstimatedDeliveryDate = estimatedDelivery,
            Status = order.Status.ToString(),
            Address = order.ShippingStreet == null ? null : new ShippingAddressDto
            {
                Street = order.ShippingStreet!,
                City = order.ShippingCity!,
                Country = order.ShippingCountry!
            },
            Items = order.Items.Select(i => new OrderItemResponseDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                Quantity = i.Quantity,
                PriceAtPurchase = i.PriceAtPurchase,
                DiscountAtPurchase = i.DiscountAtPurchase
            }).ToList()
        };
    }
}

