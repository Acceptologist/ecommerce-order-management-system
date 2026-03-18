using System.Linq;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Features.Orders.Queries.ValidateCart;

public class ValidateCartQueryHandler : IRequestHandler<ValidateCartQuery, CartValidationResultDto>
{
    private readonly IUnitOfWork _uow;

    public ValidateCartQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<CartValidationResultDto> Handle(ValidateCartQuery request, CancellationToken cancellationToken)
    {
        var result = new CartValidationResultDto { Valid = true };
        if (request.Items == null || !request.Items.Any())
            return result;

        var productRepo = _uow.Repository<Product>();
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var query = productRepo.Query().Where(p => productIds.Contains(p.Id) && !p.IsDeleted);
        var products = await productRepo.ToListAsync(query, cancellationToken);

        foreach (var item in request.Items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null)
            {
                result.Valid = false;
                result.Errors.Add(new CartItemStockErrorDto
                {
                    ProductId = item.ProductId,
                    ProductName = $"Product #{item.ProductId}",
                    Requested = item.Quantity,
                    Available = 0
                });
                continue;
            }
            if (item.Quantity <= 0 || product.StockQuantity < item.Quantity)
            {
                result.Valid = false;
                result.Errors.Add(new CartItemStockErrorDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Requested = item.Quantity,
                    Available = product.StockQuantity
                });
            }
        }

        return result;
    }
}
