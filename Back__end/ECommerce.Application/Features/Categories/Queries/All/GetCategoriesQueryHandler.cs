using System.Collections.Generic;
using ECommerce.Application.DTOs.Category;
using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Domain.Entities;
using MediatR;

// Removed EF Core import; using repository abstraction for async operations

namespace ECommerce.Application.Features.Categories.Queries.All;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<ReadCategoryDto>>
{
    private readonly IUnitOfWork _uow;

    public GetCategoriesQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IReadOnlyList<ReadCategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _uow.Repository<Category>().Query().Where(c => !c.IsDeleted);
        var categories = await _uow.Repository<Category>().ToListAsync(query, cancellationToken);
        return categories.Select(c => new ReadCategoryDto { Id = c.Id, Name = c.Name }).ToList();
    }
}

