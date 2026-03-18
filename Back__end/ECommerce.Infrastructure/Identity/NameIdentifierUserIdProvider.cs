using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ECommerce.Infrastructure.Identity;

public class NameIdentifierUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
