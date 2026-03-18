import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Product } from '../../../core/models/product.model';

export interface LowStockDialogData {
  products: Product[];
}

@Component({
  selector: 'app-low-stock-dialog',
  standalone: true,
  imports: [CommonModule, RouterModule, MatDialogModule, MatIconModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>inventory_2</mat-icon>
      Low stock &amp; out of stock
    </h2>
    <mat-dialog-content>
      <p class="subtitle">Products with fewer than 5 items in stock.</p>
      <div *ngIf="data.products.length === 0" class="empty">
        <mat-icon>check_circle</mat-icon>
        <p>No low-stock products.</p>
      </div>
      <ul class="product-list" *ngIf="data.products.length > 0">
        <li *ngFor="let p of data.products" class="product-row">
          <span class="name">{{ p.name }}</span>
          <span class="category">{{ p.categoryName }}</span>
          <span class="stock" [class.out]="p.stockQuantity === 0" [class.low]="p.stockQuantity > 0 && p.stockQuantity < 5">
            {{ p.stockQuantity === 0 ? 'Out of stock' : (p.stockQuantity + ' left') }}
          </span>
          <a mat-button [routerLink]="['/products', p.id, 'edit']" (click)="close()" color="primary">Edit</a>
        </li>
      </ul>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Close</button>
      <a mat-raised-button color="primary" routerLink="/products" (click)="close()">View all products</a>
    </mat-dialog-actions>
  `,
  styles: [`
    h2 { display: flex; align-items: center; gap: 8px; }
    .subtitle { color: #666; margin: 0 0 16px; font-size: 0.9rem; }
    .empty { text-align: center; padding: 24px; color: #666; }
    .empty mat-icon { font-size: 48px; width: 48px; height: 48px; color: #22c55e; }
    .product-list { list-style: none; padding: 0; margin: 0; max-height: 60vh; overflow-y: auto; }
    .product-row { display: flex; align-items: center; gap: 12px; padding: 12px 0; border-bottom: 1px solid #eee; flex-wrap: wrap; }
    .product-row .name { flex: 1 1 180px; font-weight: 500; }
    .product-row .category { flex: 0 0 120px; color: #666; font-size: 0.9rem; }
    .product-row .stock { flex: 0 0 100px; font-size: 0.9rem; font-weight: 600; }
    .product-row .stock.out { color: #ef4444; }
    .product-row .stock.low { color: #f59e0b; }
  `]
})
export class LowStockDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<LowStockDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: LowStockDialogData
  ) {}

  close(): void {
    this.dialogRef.close();
  }
}
