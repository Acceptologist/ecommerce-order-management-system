import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CategoryService, Category } from '../../../core/services/category.service';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog.component';
import { AddCategoryDialogComponent } from '../add-category-dialog/add-category-dialog.component';
import { NotificationService } from '../../../core/services/notification.service';

export interface ManageCategoriesDialogData {
  categories: Category[];
}

@Component({
  selector: 'app-manage-categories-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatTooltipModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>category</mat-icon>
      Manage Categories
    </h2>
    <mat-dialog-content>
      <mat-list *ngIf="categories.length > 0">
        <mat-list-item *ngFor="let cat of categories">
          <span class="cat-name">{{ cat.name }}</span>
          <button mat-icon-button (click)="edit(cat)" matTooltip="Edit">
            <mat-icon>edit</mat-icon>
          </button>
          <button mat-icon-button color="warn" (click)="deleteCat(cat)" matTooltip="Delete (soft)">
            <mat-icon>delete</mat-icon>
          </button>
        </mat-list-item>
      </mat-list>
      <p *ngIf="categories.length === 0" class="empty">No categories yet.</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close(true)">Close</button>
    </mat-dialog-actions>
  `,
  styles: [`
    h2 { display: flex; align-items: center; gap: 8px; }
    .cat-name { flex: 1; }
    mat-list-item { display: flex; align-items: center; }
    .empty { padding: 16px; color: #666; }
  `]
})
export class ManageCategoriesDialogComponent {
  categories: Category[];

  constructor(
    public dialogRef: MatDialogRef<ManageCategoriesDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ManageCategoriesDialogData,
    private categoryService: CategoryService,
    private dialog: MatDialog,
    private notificationService: NotificationService
  ) {
    this.categories = [...(data?.categories ?? [])];
  }

  refreshList(): void {
    this.categoryService.getAll().subscribe(c => {
      this.categories = c;
    });
  }

  edit(cat: Category): void {
    const ref = this.dialog.open(AddCategoryDialogComponent, {
      width: '360px',
      data: { category: cat }
    });
    ref.afterClosed().subscribe((saved: boolean) => {
      if (saved) this.refreshList();
    });
  }

  deleteCat(cat: Category): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      width: '420px',
      data: {
        title: 'Delete category',
        icon: 'delete',
        tone: 'danger',
        confirmLabel: 'Delete',
        message: `Soft-delete "${cat.name}"? It will be hidden from the category list.`
      }
    });
    ref.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) return;
      this.categoryService.delete(cat.id).subscribe({
        next: () => this.refreshList(),
        error: (err) => {
          this.notificationService.pushToast({
            id: Date.now(),
            userId: 0,
            message: err?.error?.message || 'Failed to delete category.',
            type: 'Error',
            isRead: false,
            createdAt: new Date().toISOString()
          });
        }
      });
    });
  }
}

