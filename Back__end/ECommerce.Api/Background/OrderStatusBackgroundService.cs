using ECommerce.Application.Features.Orders.Commands.UpdateStatus;
using ECommerce.Application.Interfaces.Persistence;
using ECommerce.Domain.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.Api.Background;

/// <summary>
/// Time-based status progression for demo: Pending -> Processing after 10s,
/// Processing -> Shipped after 1 min, Shipped -> Completed after 2 min from order date.
/// </summary>
public class OrderStatusBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<OrderStatusBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

    public OrderStatusBackgroundService(IServiceProvider services, ILogger<OrderStatusBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var orderRepo = uow.Repository<ECommerce.Domain.Entities.Order>();
                var query = orderRepo.Query();

                var now = DateTime.UtcNow;
                // Use cutoff dates so EF Core can translate to SQL (no TimeSpan in Where)
                var cutoffPending = now.AddSeconds(-10);
                var cutoffProcessing = now.AddMinutes(-1);
                var cutoffShipped = now.AddMinutes(-2);

                var pending = query
                    .Where(o => o.Status == OrderStatus.Pending && o.OrderDate <= cutoffPending)
                    .Select(o => o.Id)
                    .ToList();

                var processing = query
                    .Where(o => o.Status == OrderStatus.Processing && o.OrderDate <= cutoffProcessing)
                    .Select(o => o.Id)
                    .ToList();

                var shipped = query
                    .Where(o => o.Status == OrderStatus.Shipped && o.OrderDate <= cutoffShipped)
                    .Select(o => o.Id)
                    .ToList();

                foreach (var id in pending)
                {
                    await mediator.Send(new UpdateOrderStatusCommand(id, OrderStatus.Processing), stoppingToken);
                    _logger.LogInformation("Order {OrderId} -> Processing", id);
                }

                foreach (var id in processing)
                {
                    await mediator.Send(new UpdateOrderStatusCommand(id, OrderStatus.Shipped), stoppingToken);
                    _logger.LogInformation("Order {OrderId} -> Shipped", id);
                }

                foreach (var id in shipped)
                {
                    await mediator.Send(new UpdateOrderStatusCommand(id, OrderStatus.Completed), stoppingToken);
                    _logger.LogInformation("Order {OrderId} -> Completed", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OrderStatusBackgroundService tick failed");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}

