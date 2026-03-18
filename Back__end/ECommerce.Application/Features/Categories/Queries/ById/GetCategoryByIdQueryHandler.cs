using ECommerce.Application.DTOs.Category;
using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Domain.Entities;
using MediatR;

// application layer avoids EF Core dependencies by using the repository abstraction

namespace ECommerce.Application.Features.Categories.Queries.ById;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, ReadCategoryDto?>
{
    private readonly IUnitOfWork _uow;

    public GetCategoryByIdQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ReadCategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _uow.Repository<Category>().Query().Where(c => c.Id == request.Id && !c.IsDeleted);
        var list = await _uow.Repository<Category>().ToListAsync(query, cancellationToken);
        var category = list.FirstOrDefault();

        if (category == null)
        {
            return null;
        }

        return new ReadCategoryDto
        {
            Id = category.Id,
            Name = category.Name
        };
    }
}

