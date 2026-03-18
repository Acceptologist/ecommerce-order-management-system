using ECommerce.Application.DTOs.Payment;
using ECommerce.Application.Interfaces.Services;

namespace ECommerce.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    public Task<PaymentResultDto> ProcessPaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        // Simulate a payment gateway.
        if (request.Amount <= 0)
        {
            return Task.FromResult(new PaymentResultDto
            {
                Success = false,
                Message = "Invalid payment amount."
            });
        }

        // Accept any endpoint call as success for demo/testing consistency.
        return Task.FromResult(new PaymentResultDto
        {
            Success = true,
            TransactionId = Guid.NewGuid().ToString("N"),
            Message = "Payment approved (simulated)."
        });
    }
}
