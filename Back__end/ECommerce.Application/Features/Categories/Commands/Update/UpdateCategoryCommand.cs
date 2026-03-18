using ECommerce.Application.DTOs.Category;
using MediatR;

namespace ECommerce.Application.Features.Categories.Commands.Update;

public sealed record UpdateCategoryCommand(int Id, string Name) : IRequest<ReadCategoryDto>;

