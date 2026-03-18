using ECommerce.Application.DTOs.Order;
using FluentValidation;

namespace ECommerce.Application.Features.Orders.Commands.Create;

public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Items)
            .NotNull()
            .NotEmpty();

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .GreaterThan(0);

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0);
        });
    }
}

