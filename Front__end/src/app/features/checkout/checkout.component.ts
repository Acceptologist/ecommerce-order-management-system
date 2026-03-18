import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatDialog, MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CartService } from '../../core/services/cart.service';
import { OrderService } from '../../core/services/order.service';
import { PaymentService } from '../../core/services/payment.service';
import { NotificationService } from '../../core/services/notification.service';
import { CartValidationResult } from '../../core/models/order.model';

import { environment } from '../../../environments/environment';

@Component({ selector: 'app-checkout', standalone: false, templateUrl: './checkout.component.html', styleUrls: ['./checkout.component.scss'] })
export class CheckoutComponent implements OnInit {
  form = this.fb.group({
    street:  ['', Validators.required],
    city:    ['', Validators.required],
    country: ['', Validators.required],
    cardHolder: [environment.testCardHolder, Validators.required],
    cardNumber: [environment.testCardNumber, [Validators.required, Validators.minLength(16)]],
    expiration: [environment.testExpiration, Validators.required],
    cvv: [environment.testCvv, [Validators.required, Validators.minLength(3)]]
  });
  loading = false;
  validatingCart = false;
  cartValidation: CartValidationResult | null = null;
  processingStage: 'idle' | 'payment' | 'order' | 'done' = 'idle';

  constructor(
    private fb: FormBuilder,
    private cart: CartService,
    private orderService: OrderService,
    private paymentService: PaymentService,
    private router: Router,
    private dialog: MatDialog,
    private notificationService: NotificationService
  ) {}

  get items() { return this.cart.items(); }
  get subtotal() { return this.cart.subtotal(); }
  get discount() { return this.cart.discount(); }
  get total() { return this.cart.total(); }
  get processingMessage(): string {
    switch (this.processingStage) {
      case 'payment':
        return 'Processing payment...';
      case 'order':
        return 'Creating your order...';
      case 'done':
        return 'Order placed successfully!';
      default:
        return 'Placing your order...';
    }
  }

  get canPlaceOrder(): boolean {
    return !this.loading && !this.validatingCart && (!this.cartValidation || this.cartValidation.valid);
  }

  ngOnInit(): void {
    this.runValidateCart();
  }

  runValidateCart(): void {
    if (this.items.length === 0) {
      this.cartValidation = { valid: true, errors: [] };
      return;
    }
    this.validatingCart = true;
    this.cartValidation = null;
    this.orderService.validateCart(this.items.map(i => ({ productId: i.product.id, quantity: i.quantity }))).subscribe({
      next: (result) => {
        this.validatingCart = false;
        this.cartValidation = result;
      },
      error: () => {
        this.validatingCart = false;
        this.cartValidation = { valid: true, errors: [] };
      }
    });
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.items.length === 0) {
      this.notificationService.pushToast({
        id: Date.now(),
        userId: 0,
        message: 'Your cart is empty. Add items before placing an order.',
        type: 'Warning',
        isRead: false,
        createdAt: new Date().toISOString()
      });
      return;
    }
    if (this.form.invalid) {
      this.notificationService.pushToast({
        id: Date.now(),
        userId: 0,
        message: 'Please complete the shipping and payment details to continue.',
        type: 'Warning',
        isRead: false,
        createdAt: new Date().toISOString()
      });
      return;
    }
    if (!this.cartValidation?.valid) {
      this.notificationService.pushToast({
        id: Date.now(),
        userId: 0,
        message: 'Some items are out of stock or the quantity is no longer available. Please update your cart below.',
        type: 'Warning',
        isRead: false,
        createdAt: new Date().toISOString()
      });
      return;
    }

    // Show confirmation dialog
    const dialogRef = this.dialog.open(OrderConfirmationDialogComponent, {
      width: '400px',
      data: {
        total: this.total,
        subtotal: this.subtotal,
        itemCount: this.items.length,
        discount: this.discount,
        discountLabel: 'Product Discounts'
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.placeOrder();
      }
    });
  }

  private placeOrder(): void {
    this.loading = true;
    this.processingStage = 'payment';
    const { street, city, country, cardHolder, cardNumber, expiration, cvv } = this.form.value;

    const paymentRequest = {
      amount: this.total,
      currency: 'USD',
      method: 'CARD',
      cardNumber: cardNumber!,
      cardHolder: cardHolder!,
      expiration: expiration!,
      cvv: cvv!
    };

    this.paymentService.simulatePayment(paymentRequest).subscribe({
      next: (payment) => {
        if (!payment.success) {
          this.loading = false;
          this.processingStage = 'idle';
          this.notificationService.pushToast({
            id: Date.now(),
            userId: 0,
            message: payment.message || 'Payment failed during simulation.',
            type: 'Error',
            isRead: false,
            createdAt: new Date().toISOString()
          });
          return;
        }

        this.processingStage = 'order';
        this.orderService.createOrder({
          items: this.items.map(i => ({ productId: i.product.id, quantity: i.quantity })),
          address: { street: street!, city: city!, country: country! }
        }).subscribe({
          next: (order) => {
            this.loading = false;
            this.processingStage = 'done';
            this.cart.clear();

            // Immediate toast so user sees success without opening notification center
            this.notificationService.pushToast({
              id: order.id + 1000000,
              userId: 0,
              message: `Order #${order.id} placed successfully. You can track it in My Orders.`,
              type: 'Success',
              isRead: false,
              createdAt: new Date().toISOString(),
              orderId: order.id
            });
            this.router.navigate(['/orders', order.id]);
          },
          error: (err) => {
            this.loading = false;
            this.processingStage = 'idle';
            this.notificationService.pushToast({
              id: Date.now(),
              userId: 0,
              message: err?.error?.message || 'Failed to place order. Please try again.',
              type: 'Error',
              isRead: false,
              createdAt: new Date().toISOString()
            });
          }
        });
      },
      error: (err) => {
        this.loading = false;
        this.processingStage = 'idle';
        this.notificationService.pushToast({
          id: Date.now(),
          userId: 0,
          message: err?.error?.message || 'Payment request failed.',
          type: 'Error',
          isRead: false,
          createdAt: new Date().toISOString()
        });
      }
    });
  }
}



@Component({
  selector: 'app-order-confirmation-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatIconModule,
    MatButtonModule,
    CommonModule
  ],
  template: `
    <h2 mat-dialog-title>Confirm Order</h2>
    <mat-dialog-content>
      <div class="confirmation-message">
        <p>You are about to place an order with:</p>
        <ul>
          <li><strong>Items:</strong> {{ data.itemCount }}</li>
          <li><strong>Subtotal:</strong> {{ data.subtotal | currency }}</li>
          <li *ngIf="data.discount > 0"><strong>Discount ({{ data.discountLabel }}):</strong> -{{ data.discount | currency }}</li>
          <li class="total"><strong>Total:</strong> {{ data.total | currency }}</li>
        </ul>
        <p class="confirmation-text">Proceed with placing this order?</p>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" (click)="onConfirm()">
        <mat-icon>check</mat-icon>
        Place Order
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .confirmation-message ul {
      list-style: none;
      padding: 0;
      margin: 16px 0;
    }
    .confirmation-message li {
      padding: 8px 0;
      border-bottom: 1px solid #eee;
    }
    .confirmation-message li.total {
      font-size: 1.1rem;
      border-bottom: 2px solid #3f51b5;
      margin-bottom: 16px;
    }
    .confirmation-text {
      margin-top: 16px;
      font-weight: 500;
    }
    mat-dialog-actions {
      margin-top: 24px;
    }
  `]
})
export class OrderConfirmationDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<OrderConfirmationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {}

  onConfirm(): void {
    this.dialogRef.close(true);
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
