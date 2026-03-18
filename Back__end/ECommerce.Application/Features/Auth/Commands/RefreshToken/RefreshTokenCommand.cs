using ECommerce.Application.DTOs.Auth;
using MediatR;

namespace ECommerce.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(RefreshTokenRequestDto Request) : IRequest<TokenResponseDto>;

