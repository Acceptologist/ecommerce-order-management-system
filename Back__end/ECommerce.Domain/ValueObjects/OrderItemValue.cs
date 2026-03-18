namespace ECommerce.Domain.ValueObjects;

public class OrderItemValue
{
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public Money PriceAtPurchase { get; private set; }

    private OrderItemValue() { }

    public OrderItemValue(int productId, int quantity, Money priceAtPurchase)
    {
        ProductId = productId;
        Quantity = quantity;
        PriceAtPurchase = priceAtPurchase;
    }
}

