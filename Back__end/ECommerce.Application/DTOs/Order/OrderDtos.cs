namespace ECommerce.Application.DTOs.Order;

public class OrderItemRequestDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class ShippingAddressDto
{
    public string Street  { get; set; } = default!;
    public string City    { get; set; } = default!;
    public string Country { get; set; } = default!;
}

public class CreateOrderRequest
{
    public List<OrderItemRequestDto> Items   { get; set; } = new();
    public ShippingAddressDto?       Address { get; set; }
}

public class ValidateCartRequest
{
    public List<OrderItemRequestDto> Items { get; set; } = new();
}

public class OrderItemResponseDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal PriceAtPurchase { get; set; }
    public decimal DiscountAtPurchase { get; set; }
}

public class CartItemStockErrorDto
{
    public int    ProductId   { get; set; }
    public string ProductName { get; set; } = default!;
    public int    Requested   { get; set; }
    public int    Available  { get; set; }
}

public class CartValidationResultDto
{
    public bool Valid { get; set; }
    public List<CartItemStockErrorDto> Errors { get; set; } = new();
}

public class OrderResponseDto
{
    public int                        Id                    { get; set; }
    public int?                       UserId                { get; set; }
    public string?                   UserName              { get; set; }
    public decimal                    Subtotal              { get; set; }
    public decimal                   TotalAmount           { get; set; }
    public decimal                   DiscountApplied       { get; set; }
    public string?                   DiscountDescription   { get; set; }
    public int                       ItemsCount            { get; set; }
    public int                       ItemsQuantity         { get; set; }
    public DateTime                  OrderDate             { get; set; }
    public DateTime?                 EstimatedDeliveryDate  { get; set; }
    public string                    Status                { get; set; } = default!;
    public ShippingAddressDto?       Address               { get; set; }
    public List<OrderItemResponseDto> Items                { get; set; } = new();
}

