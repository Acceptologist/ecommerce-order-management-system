using ECommerce.Application.DTOs.Category;
using MediatR;

namespace ECommerce.Application.Features.Categories.Commands.Create;

public sealed record CreateCategoryCommand(string Name) : IRequest<ReadCategoryDto>;

