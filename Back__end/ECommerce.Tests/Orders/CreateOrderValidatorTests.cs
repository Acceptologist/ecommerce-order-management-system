using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Features.Orders.Commands.Create;
using FluentAssertions;

namespace ECommerce.Tests.Orders;

public class CreateOrderValidatorTests
{
    private readonly CreateOrderValidator _validator = new();

    [Fact]
    public async Task ValidRequest_Passes()
    {
        var request = new CreateOrderRequest
        {
            Items = [new() { ProductId = 1, Quantity = 2 }]
        };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task EmptyItems_Fails()
    {
        var result = await _validator.ValidateAsync(new CreateOrderRequest { Items = [] });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ZeroProductId_Fails()
    {
        var result = await _validator.ValidateAsync(new CreateOrderRequest
        {
            Items = [new() { ProductId = 0, Quantity = 1 }]
        });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ZeroQuantity_Fails()
    {
        var result = await _validator.ValidateAsync(new CreateOrderRequest
        {
            Items = [new() { ProductId = 1, Quantity = 0 }]
        });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task NegativeQuantity_Fails()
    {
        var result = await _validator.ValidateAsync(new CreateOrderRequest
        {
            Items = [new() { ProductId = 1, Quantity = -1 }]
        });
        result.IsValid.Should().BeFalse();
    }
}
