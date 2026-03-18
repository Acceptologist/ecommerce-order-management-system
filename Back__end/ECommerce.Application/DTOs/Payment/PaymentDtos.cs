namespace ECommerce.Application.DTOs.Payment;

public class PaymentRequestDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Method { get; set; } = "CARD";
    public string? CardNumber { get; set; }
    public string? CardHolder { get; set; }
    public string? Expiration { get; set; }
    public string? Cvv { get; set; }
}

public class PaymentResultDto
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? Message { get; set; }
}
