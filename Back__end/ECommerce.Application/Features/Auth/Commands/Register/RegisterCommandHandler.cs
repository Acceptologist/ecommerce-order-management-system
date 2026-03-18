using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.DTOs.Auth;
using MediatR;

namespace ECommerce.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, TokenResponseDto>
{
    private readonly IAuthService _authService;

    public RegisterCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public Task<TokenResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken) =>
        _authService.RegisterAsync(request.Request, cancellationToken);
}

