using FluentValidation;

namespace ECommerce.Application.Features.Categories.Commands.Update;

public sealed class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();
    }
}

