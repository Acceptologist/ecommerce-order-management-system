using Microsoft.AspNetCore.Identity;

namespace ECommerce.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string? DisplayName { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}

