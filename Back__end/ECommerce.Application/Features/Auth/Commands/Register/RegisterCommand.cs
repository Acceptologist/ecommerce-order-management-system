using ECommerce.Application.DTOs.Auth;
using MediatR;

namespace ECommerce.Application.Features.Auth.Commands.Register;

public record RegisterCommand(RegisterRequestDto Request) : IRequest<TokenResponseDto>;

