using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Domain.Entities;
using MediatR;

// keep application layer free of EF imports

namespace ECommerce.Application.Features.Products.Queries.ById;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ReadProductDto?>
{
    private readonly IUnitOfWork _uow;

    public GetProductByIdQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ReadProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _uow.Repository<Product>().Query().Where(p => p.Id == request.Id && !p.IsDeleted);
        query = Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include(query, p => p.Category);

        var products = await _uow.Repository<Product>().ToListAsync(query, cancellationToken);
        var product = products.FirstOrDefault();
        if (product is null)
        {
            return null;
        }

        return new ReadProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            ImageUrl = product.ImageUrl,
            DiscountRate = product.DiscountRate
        };
    }
}

