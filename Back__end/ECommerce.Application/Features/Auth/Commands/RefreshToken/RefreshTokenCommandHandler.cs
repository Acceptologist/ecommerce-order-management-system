using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.DTOs.Auth;
using MediatR;

namespace ECommerce.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenResponseDto>
{
    private readonly IAuthService _authService;

    public RefreshTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public Task<TokenResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken) =>
        _authService.RefreshAsync(request.Request.RefreshToken, cancellationToken);
}

