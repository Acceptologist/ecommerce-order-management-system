using ECommerce.Application.DTOs.Payment;
using ECommerce.Infrastructure.Services;
using FluentAssertions;

namespace ECommerce.Tests.Payments;

public class PaymentServiceTests
{
    [Fact]
    public async Task ProcessPaymentAsync_PositiveAmount_ReturnsSuccess()
    {
        var service = new PaymentService();

        var result = await service.ProcessPaymentAsync(new PaymentRequestDto
        {
            Amount = 10m,
            Currency = "USD",
            Method = "CARD"
        });

        result.Success.Should().BeTrue();
        result.TransactionId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ProcessPaymentAsync_NonPositiveAmount_ReturnsFailure()
    {
        var service = new PaymentService();

        var result = await service.ProcessPaymentAsync(new PaymentRequestDto
        {
            Amount = 0m,
            Currency = "USD",
            Method = "CARD"
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid payment amount");
    }
}
