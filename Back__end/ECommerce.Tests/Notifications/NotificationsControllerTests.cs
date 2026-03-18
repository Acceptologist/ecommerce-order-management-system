using System.Security.Claims;
using ECommerce.Api.Controllers;
using ECommerce.Application.DTOs.Notification;
using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Tests.Notifications;

public class NotificationsControllerTests
{
    private static async Task<(AppDbContext db, UnitOfWork uow)> CreateDbAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);

        await db.SaveChangesAsync();
        var uow = new UnitOfWork(db);
        return (db, uow);
    }

    private static NotificationsController CreateController(int userId, IUnitOfWork uow)
    {
        var ctrl = new NotificationsController(uow);
        ctrl.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }, "TestAuth"))
            }
        };
        return ctrl;
    }

    [Fact]
    public async Task Get_ReturnsUserNotificationsAndUnreadCountUpdates()
    {
        var (db, uow) = await CreateDbAsync();
        db.Notifications.AddRange(
            new Notification { UserId = 1, Message = "A", Type = "Info", IsRead = false, CreatedAt = DateTime.UtcNow },
            new Notification { UserId = 1, Message = "B", Type = "Info", IsRead = true, CreatedAt = DateTime.UtcNow },
            new Notification { UserId = 2, Message = "C", Type = "Info", IsRead = false, CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var controller = CreateController(1, uow);

        var getResult = await controller.Get(CancellationToken.None) as OkObjectResult;
        getResult.Should().NotBeNull();
        var list = getResult!.Value as IEnumerable<NotificationDto>;
        list.Should().NotBeNull();
        list.Should().HaveCount(2);

        var unreadResult = await controller.GetUnreadCount(CancellationToken.None) as OkObjectResult;
        unreadResult.Should().NotBeNull();
        var payload = unreadResult!.Value;
        payload.Should().NotBeNull();

        if (payload is IDictionary<string, object> map)
        {
            ((int)map["count"]).Should().Be(1);
        }
        else
        {
            var countProperty = payload.GetType().GetProperty("count") ?? payload.GetType().GetProperty("Count");
            countProperty.Should().NotBeNull();
            var count = (int?)countProperty!.GetValue(payload);
            count.Should().Be(1);
        }
    }

    [Fact]
    public async Task MarkAsRead_SetsIsReadToTrue()
    {
        var (db, uow) = await CreateDbAsync();
        var notification = new Notification { UserId = 1, Message = "X", Type = "Info", IsRead = false, CreatedAt = DateTime.UtcNow };
        db.Notifications.Add(notification);
        await db.SaveChangesAsync();

        var controller = CreateController(1, uow);

        var result = await controller.MarkAsRead(notification.Id, CancellationToken.None);
        result.Should().BeOfType<NoContentResult>();

        var updated = await db.Notifications.FindAsync(notification.Id);
        updated.Should().NotBeNull();
        updated!.IsRead.Should().BeTrue();
    }
}
