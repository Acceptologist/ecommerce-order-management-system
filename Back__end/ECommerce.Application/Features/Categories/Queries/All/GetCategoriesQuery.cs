using System.Collections.Generic;
using ECommerce.Application.DTOs.Category;
using MediatR;

namespace ECommerce.Application.Features.Categories.Queries.All;

public record GetCategoriesQuery() : IRequest<IReadOnlyList<ReadCategoryDto>>;

