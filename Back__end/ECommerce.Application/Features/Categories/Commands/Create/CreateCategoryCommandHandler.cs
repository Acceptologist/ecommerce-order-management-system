using ECommerce.Application.DTOs.Category;
using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Features.Categories.Commands.Create;

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, ReadCategoryDto>
{
    private readonly IUnitOfWork _uow;

    public CreateCategoryCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ReadCategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = new Category
        {
            Name = request.Name.Trim()
        };

        await _uow.Repository<Category>().AddAsync(entity, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new ReadCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }
}

