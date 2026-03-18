export interface PaymentRequest {
  amount: number;
  currency: string;
  method: string;
  cardNumber?: string;
  cardHolder?: string;
  expiration?: string;
  cvv?: string;
}

export interface PaymentResult {
  success: boolean;
  transactionId?: string;
  message?: string;
}
