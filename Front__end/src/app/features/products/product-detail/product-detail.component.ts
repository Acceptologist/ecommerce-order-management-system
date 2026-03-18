import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';
import { CartService } from '../../../core/services/cart.service';
import { Product } from '../../../core/models/product.model';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-product-detail',
  standalone: false,
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.scss']
})
export class ProductDetailComponent implements OnInit {
  product?: Product;
  quantity = 1;
  loading = false;

  constructor(
    private route: ActivatedRoute,
    private productService: ProductService,
    public cart: CartService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.loading = true;
      this.productService.getById(id).subscribe({
        next: (p) => {
          this.product = p;
          this.loading = false;
        },
        error: (err) => {
          console.error('Failed to load product:', err);
          this.loading = false;
        }
      });
    }
  }

  addToCart(): void {
    if (this.product && this.quantity > 0 && this.product.stockQuantity > 0) {
      this.cart.addItem(this.product, this.quantity);
      // Show success toast
      this.notificationService.pushToast({
        id: Date.now(),
        userId: 0,
        message: `${this.quantity} × ${this.product.name} added to cart`,
        type: 'Success',
        isRead: false,
        createdAt: new Date().toISOString()
      });
      this.quantity = 1;
    }
  }

  getStockBadgeClass(quantity: number): string {
    if (quantity === 0) return 'out-of-stock';
    if (quantity < 5) return 'low-stock';
    return 'in-stock';
  }

  getStockLabel(quantity: number): string {
    if (quantity === 0) return 'Out of Stock';
    if (quantity < 5) return `Only ${quantity} left!`;
    return `In Stock (${quantity} available)`;
  }

  getProductImage(product: Product): string {
    return product.imageUrl?.trim() || 'assets/images/placeholder.png';
  }
}
