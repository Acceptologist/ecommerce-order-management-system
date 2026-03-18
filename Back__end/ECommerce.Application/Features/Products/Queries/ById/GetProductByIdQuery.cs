using ECommerce.Application.DTOs.Product;
using MediatR;

namespace ECommerce.Application.Features.Products.Queries.ById;

public sealed record GetProductByIdQuery(int Id) : IRequest<ReadProductDto?>;

