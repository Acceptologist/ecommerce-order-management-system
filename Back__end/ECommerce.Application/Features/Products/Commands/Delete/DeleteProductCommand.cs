using MediatR;

namespace ECommerce.Application.Features.Products.Commands.Delete;

public record DeleteProductCommand(int Id) : IRequest;

