using ECommerce.Application.DTOs.Category;
using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Features.Categories.Commands.Update;

public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, ReadCategoryDto>
{
    private readonly IUnitOfWork _uow;

    public UpdateCategoryCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ReadCategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var repo = _uow.Repository<Category>();
        var existing = await repo.GetByIdAsync(request.Id, cancellationToken);
        if (existing is null || existing.IsDeleted)
        {
            throw new KeyNotFoundException($"Category with id {request.Id} was not found.");
        }

        existing.Name = request.Name.Trim();
        repo.Update(existing);
        await _uow.SaveChangesAsync(cancellationToken);

        return new ReadCategoryDto
        {
            Id = existing.Id,
            Name = existing.Name
        };
    }
}

