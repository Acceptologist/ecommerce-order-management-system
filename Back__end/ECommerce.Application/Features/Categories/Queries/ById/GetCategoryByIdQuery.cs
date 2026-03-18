using ECommerce.Application.DTOs.Category;
using MediatR;

namespace ECommerce.Application.Features.Categories.Queries.ById;

public record GetCategoryByIdQuery(int Id) : IRequest<ReadCategoryDto?>;

