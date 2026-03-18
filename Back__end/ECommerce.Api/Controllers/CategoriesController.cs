using ECommerce.Application.DTOs.Category;
using ECommerce.Application.Features.Categories.Commands.Create;
using ECommerce.Application.Features.Categories.Commands.Delete;
using ECommerce.Application.Features.Categories.Commands.Update;
using ECommerce.Application.Features.Categories.Queries.All;
using ECommerce.Application.Features.Categories.Queries.ById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReadCategoryDto>>> Get(CancellationToken cancellationToken)
    {
        var items = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReadCategoryDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _mediator.Send(new GetCategoryByIdQuery(id), cancellationToken);
        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ReadCategoryDto>> Create([FromBody] CreateCategoryRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateCategoryCommand(request.Name), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ReadCategoryDto>> Update(int id, [FromBody] UpdateCategoryRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateCategoryCommand(id, request.Name), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
        return NoContent();
    }
}
