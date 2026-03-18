using FluentValidation;

namespace ECommerce.Application.Features.Categories.Commands.Create;

public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();
    }
}

