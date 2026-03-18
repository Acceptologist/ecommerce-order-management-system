using ECommerce.Application.Features.Orders.Commands.Create;
using ECommerce.Application.Features.Orders.Commands.CancelOrder;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Features.Orders.Queries.All;
using ECommerce.Application.Features.Orders.Queries.ById;
using ECommerce.Application.Features.Orders.Queries.ValidateCart;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("validate-cart")]
    public async Task<ActionResult<CartValidationResultDto>> ValidateCart([FromBody] ValidateCartRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ValidateCartQuery(request.Items ?? new List<OrderItemRequestDto>()), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponseDto>> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _mediator.Send(new CreateOrderCommand(userId, request), cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool desc = true,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var result = await _mediator.Send(new GetOrdersQuery(userId, page, pageSize, search, status, startDate, endDate, sortBy, desc), cancellationToken);
        return Ok(result);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool desc = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetOrdersAllQuery(page, pageSize, search, status, startDate, endDate, sortBy, desc), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (User.IsInRole("Admin"))
        {
            var adminResult = await _mediator.Send(new GetOrderByIdForAdminQuery(id), cancellationToken);
            if (adminResult == null) return NotFound();
            return Ok(adminResult);
        }
        var result = await _mediator.Send(new GetOrderByIdQuery(userId, id), cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelOrderCommand(id), cancellationToken);
        return NoContent();
    }
}
