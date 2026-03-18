using ECommerce.Application.DTOs.Product;
using MediatR;

namespace ECommerce.Application.Features.Products.Commands.Create;

public record CreateProductCommand(CreateProductDto Dto) : IRequest<int>;

