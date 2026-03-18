using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Features.Orders.Queries.ById;

public sealed class GetOrderByIdForAdminQueryHandler : IRequestHandler<GetOrderByIdForAdminQuery, OrderResponseDto?>
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _userRepo;

    public GetOrderByIdForAdminQueryHandler(IUnitOfWork uow, IUserRepository userRepo)
    {
        _uow = uow;
        _userRepo = userRepo;
    }

    public async Task<OrderResponseDto?> Handle(GetOrderByIdForAdminQuery request, CancellationToken cancellationToken)
    {
        var query = _uow.Repository<Order>().Query();
        var included = Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include(query, (Order o) => o.Items);
        query = Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ThenInclude<Order, OrderItem, Product>(included, i => i.Product);
        query = query.Where(o => o.Id == request.OrderId);

        var orders = await _uow.Repository<Order>().ToListAsync(query, cancellationToken);
        var order = orders.FirstOrDefault();

        if (order is null)
            return null;

        var userNames = await _userRepo.GetUserNamesByIdsAsync(new[] { order.UserId }, cancellationToken);
        var userName = userNames.ContainsKey(order.UserId) ? userNames[order.UserId] : null;

        var subtotal = order.Items.Sum(i => i.PriceAtPurchase * i.Quantity);
        var estimatedDelivery = order.OrderDate.AddDays(7);

        return new OrderResponseDto
        {
            Id = order.Id,
            UserId = order.UserId,
            UserName = userName,
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
