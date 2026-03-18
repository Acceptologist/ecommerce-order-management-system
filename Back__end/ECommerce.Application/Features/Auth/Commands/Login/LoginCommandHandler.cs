using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.DTOs.Auth;
using MediatR;

namespace ECommerce.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, TokenResponseDto>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public Task<TokenResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken) =>
        _authService.LoginAsync(request.Request, cancellationToken);
}

