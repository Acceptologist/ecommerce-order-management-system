using ECommerce.Application.DTOs.Product;
using MediatR;

namespace ECommerce.Application.Features.Products.Commands.Update;

public record UpdateProductCommand(int Id, UpdateProductDto Dto) : IRequest;

