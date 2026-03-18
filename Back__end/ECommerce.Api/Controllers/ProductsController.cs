using ECommerce.Application.DTOs.Generic;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Features.Products.Queries.All;
using ECommerce.Application.Features.Products.Commands.Create;
using ECommerce.Application.Features.Products.Commands.Update;
using ECommerce.Application.Features.Products.Commands.Delete;
using ECommerce.Domain.Entities;
using ECommerce.Application.Features.Products.Queries.ById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductListItemDto>>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool desc = false)
    {
        var result = await _mediator.Send(new GetProductsPagedQuery(page, pageSize, search, categoryId, sortBy, desc));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReadProductDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var dto = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        if (dto == null)
        {
            return NotFound();
        }
        return Ok(dto);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateProductCommand(dto), cancellationToken);
        var createdDto = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, createdDto);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateProductCommand(id, dto), cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return NoContent();
    }
}
