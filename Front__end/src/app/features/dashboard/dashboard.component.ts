import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { OrderService } from '../../core/services/order.service';
import { ProductService } from '../../core/services/product.service';
import { AuthService } from '../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { Product } from '../../core/models/product.model';

// material modules required for standalone component
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { RouterModule } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';
import { LowStockDialogComponent } from './low-stock-dialog/low-stock-dialog.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatIconModule,
    MatTableModule,
    MatChipsModule,
    MatProgressBarModule,
    MatListModule,
    MatButtonModule,
    MatDialogModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(12px)' }),
        animate('400ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ])
  ]
})
export class DashboardComponent implements OnInit {
  totalSales = 0;
  ordersToday = 0;
  lowStock = 0;
  conversionRate = 0;
  loading = false;
  lowStockProducts: Product[] = [];

  constructor(
    private orderService: OrderService,
    private productService: ProductService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef,
    private dialog: MatDialog
  ) {}

  recentOrders: any[] = [];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;

    const ordersRequest$ = this.authService.isAdmin 
      ? this.orderService.getAllOrders(1, 1000) 
      : this.orderService.getOrders(1, 1000);

    forkJoin({
      ordersRes: ordersRequest$,
      products: this.productService.getProducts(1, 1000)
    }).pipe(
      finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: ({ ordersRes, products }) => {
        const orders = ordersRes.items || [];
        const today = new Date().toDateString();
        this.ordersToday = orders.filter(o => new Date(o.orderDate).toDateString() === today).length;
        this.totalSales = orders.reduce((sum, o) => sum + (o.totalAmount || 0), 0);
        this.conversionRate = orders.length ? Math.min(1, orders.length / 100) * 100 : 0;
        this.recentOrders = orders
          .slice() // don’t mutate original array from service (best practice)
          .sort((a, b) => new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime())
          .slice(0, 5);

        this.lowStockProducts = products.items.filter(p => p.stockQuantity < 5);
        this.lowStock = this.lowStockProducts.length;
      },
      error: () => {
        // loading handled in finalize
      }
    });
  }

  statusColor(status: string | undefined): string {
    if (!status) { return ''; }
    switch (status.toLowerCase()) {
      case 'pending': return 'warn';
      case 'processing': return 'primary';
      case 'shipped': return 'accent';
      case 'completed': return 'primary';
      case 'cancelled': return 'accent';
      default: return '';
    }
  }

  openLowStockDialog(): void {
    this.dialog.open(LowStockDialogComponent, {
      width: '560px',
      maxHeight: '90vh',
      data: { products: this.lowStockProducts }
    });
  }
}
