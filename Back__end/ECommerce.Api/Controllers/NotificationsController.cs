using ECommerce.Application.DTOs.Notification;
using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ApiControllerBase
{
    private readonly IUnitOfWork _uow;

    public NotificationsController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var list = await _uow.Repository<Notification>().Query()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                OrderId = n.OrderId
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var count = await _uow.Repository<Notification>().Query()
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync(cancellationToken);

        return Ok(new { count });
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var repo = _uow.Repository<Notification>();
        var notif = await repo.Query()
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, cancellationToken);

        if (notif == null)
        {
            return NotFound();
        }

        notif.IsRead = true;
        repo.Update(notif);
        await _uow.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
