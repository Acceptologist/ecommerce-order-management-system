using ECommerce.Application.DTOs.Payment;

namespace ECommerce.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<PaymentResultDto> ProcessPaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken = default);
}
