using ECommerce.Application.DTOs.Auth;
using FluentValidation;

namespace ECommerce.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .MaximumLength(500);
    }
}

