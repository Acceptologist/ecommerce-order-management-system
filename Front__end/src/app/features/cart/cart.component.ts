import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-cart',
  standalone: false,
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.scss']
})
export class CartComponent {
  constructor(
    public cart: CartService, 
    private router: Router,
    private authService: AuthService
  ) {}

  checkout(): void {
    if (this.cart.items().length > 0) {
      if (this.authService.isLoggedIn) {
        this.router.navigate(['/checkout']);
      } else {
        // Redirect to login, and optionally pass the returnUrl so they come back to checkout
        this.router.navigate(['/login'], { queryParams: { returnUrl: '/checkout' } });
      }
    }
  }

  removeItem(productId: number): void {
    this.cart.removeItem(productId);
  }

  updateQuantity(productId: number, quantity: number): void {
    this.cart.updateQuantity(productId, quantity);
  }

  trackByProductId(_: number, item: any): number {
    return item.product.id;
  }
}
