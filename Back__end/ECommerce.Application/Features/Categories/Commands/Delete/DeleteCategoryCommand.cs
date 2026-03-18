using MediatR;

namespace ECommerce.Application.Features.Categories.Commands.Delete;

public sealed record DeleteCategoryCommand(int Id) : IRequest;

