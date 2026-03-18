export interface OrderItemRequest { productId: number; quantity: number; }
export interface ShippingAddress  { street: string; city: string; country: string; }
export interface CreateOrderRequest { items: OrderItemRequest[]; address?: ShippingAddress; }

export interface CartItemStockError {
  productId: number;
  productName: string;
  requested: number;
  available: number;
}
export interface CartValidationResult {
  valid: boolean;
  errors: CartItemStockError[];
}
export interface OrderItemResponse  { productId: number; productName: string; quantity: number; priceAtPurchase: number; discountAtPurchase: number; }
export interface OrderResponse {
  id: number;
  userId?: number;
  userName?: string;
  subtotal?: number;
  totalAmount: number;
  discountApplied: number;
  discountDescription?: string;
  itemsCount: number;
  itemsQuantity: number;
  orderDate: string;
  estimatedDeliveryDate?: string;
  status: string;
  address?: ShippingAddress;
  items: OrderItemResponse[];
}
