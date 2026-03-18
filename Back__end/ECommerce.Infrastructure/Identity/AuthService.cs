using System.Security.Claims;
using System.Security.Cryptography;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ITokenService _tokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        AppDbContext db,
        IConfiguration configuration,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
        _configuration = configuration;
        _tokenService = tokenService;
    }

    public async Task<TokenResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var existing = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == request.Username, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException("Username already exists");
        }

        await EnsureRoleExistsAsync("User");
        await EnsureRoleExistsAsync("Admin");

        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = string.IsNullOrWhiteSpace(request.Email) ? $"{request.Username}@local.test" : request.Email,
            DisplayName = request.Username
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var msg = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(msg);
        }

        await _userManager.AddToRoleAsync(user, "User");

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<TokenResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.UserName == request.Username, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var valid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!valid)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<TokenResponseDto> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _db.RefreshTokens
            .Include(t => t.User)
            .ThenInclude(u => u.RefreshTokens)
            .FirstOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);

        if (token == null || !token.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var user = token.User;

        // Rotate refresh token
        var newTokenString = _tokenService.GenerateRefreshToken();
        var newRefresh = new RefreshToken
        {
            UserId = user.Id,
            Token = newTokenString,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(14)
        };
        token.RevokedAt = DateTime.UtcNow;
        token.ReplacedByToken = newRefresh.Token;
        user.RefreshTokens.Add(newRefresh);

        await _db.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);
        var access = _tokenService.GenerateAccessToken(user.Id.ToString(), user.UserName ?? string.Empty, roles);

        return new TokenResponseDto
        {
            AccessToken = access,
            RefreshToken = newRefresh.Token,
            Username = user.UserName ?? "",
            Roles = roles.ToArray()
        };
    }

    private async Task<TokenResponseDto> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user.Id.ToString(), user.UserName ?? string.Empty, roles);
        var refreshTokenString = _tokenService.GenerateRefreshToken();
        var refresh = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(14)
        };

        user.RefreshTokens.Add(refresh);
        await _db.SaveChangesAsync(cancellationToken);

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refresh.Token,
            Username = user.UserName ?? "",
            Roles = roles.ToArray()
        };
    }



    private async Task EnsureRoleExistsAsync(string role)
    {
        if (await _roleManager.RoleExistsAsync(role))
        {
            return;
        }
        await _roleManager.CreateAsync(new IdentityRole<int>(role));
    }
}

