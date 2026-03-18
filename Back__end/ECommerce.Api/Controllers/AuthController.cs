using ECommerce.Application.Features.Auth.Commands.Login;
using ECommerce.Application.Features.Auth.Commands.RefreshToken;
using ECommerce.Application.Features.Auth.Commands.Register;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;
    private readonly IRevokedTokenStore _revokedTokenStore;

    public AuthController(IMediator mediator, ILogger<AuthController> logger, IRevokedTokenStore revokedTokenStore)
    {
        _mediator = mediator;
        _logger = logger;
        _revokedTokenStore = revokedTokenStore;
    }

    [HttpPost("register")]
    public async Task<ActionResult<TokenResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        _logger.LogInformation("Registration attempt for username: {Username}", request.Username);
        try
        {
            var result = await _mediator.Send(new RegisterCommand(request));
            _logger.LogInformation("Registration successful for username: {Username}", request.Username);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for username: {Username}", request.Username);
            throw;
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        _logger.LogInformation("Login attempt for username: {Username}", request.Username);
        try
        {
            var result = await _mediator.Send(new LoginCommand(request));
            _logger.LogInformation("Login successful for username: {Username}", request.Username);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for username: {Username}", request.Username);
            throw;
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponseDto>> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(request));
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        var token = Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(token))
        {
            _revokedTokenStore.Revoke(token);
            _logger.LogInformation("User logged out; token revoked.");
        }
        return NoContent();
    }
}
