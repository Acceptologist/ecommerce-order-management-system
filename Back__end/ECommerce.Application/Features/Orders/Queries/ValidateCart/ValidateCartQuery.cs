using ECommerce.Application.DTOs.Order;
using MediatR;

namespace ECommerce.Application.Features.Orders.Queries.ValidateCart;

public record ValidateCartQuery(List<OrderItemRequestDto> Items) : IRequest<CartValidationResultDto>;
