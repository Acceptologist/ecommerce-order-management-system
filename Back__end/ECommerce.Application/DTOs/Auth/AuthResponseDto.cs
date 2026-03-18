namespace ECommerce.Application.DTOs.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public string Username { get; set; } = default!;
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
}

