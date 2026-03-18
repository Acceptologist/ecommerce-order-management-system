using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ApiControllerBase
{
    private readonly UserManager<Microsoft.AspNetCore.Identity.IdentityUser<int>> _userManager;

    public UsersController(UserManager<Microsoft.AspNetCore.Identity.IdentityUser<int>> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new { user.Id, Username = user.UserName, Roles = roles });
    }
}
