import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CategoryService, Category } from '../../../core/services/category.service';
import { AddCategoryDialogComponent } from '../add-category-dialog/add-category-dialog.component';
import { ManageCategoriesDialogComponent } from '../manage-categories-dialog/manage-categories-dialog.component';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-categories-page',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatTooltipModule],
  template: `
    <div class="page">
      <div class="header">
        <h1>Categories</h1>
        <div class="actions">
          <button mat-stroked-button (click)="openManage()">
            <mat-icon>folder_open</mat-icon>
            Manage
          </button>
          <button mat-raised-button color="primary" (click)="openAdd()">
            <mat-icon>add</mat-icon>
            Add Category
          </button>
        </div>
      </div>

      <mat-card class="list-card">
        <div *ngIf="loading" class="loading">Loading…</div>
        <div *ngIf="!loading && categories.length === 0" class="empty">
          No categories yet.
        </div>
        <ul *ngIf="!loading && categories.length > 0" class="list">
          <li *ngFor="let c of categories">
            <span class="name">{{ c.name }}</span>
            <div class="actions">
              <button mat-icon-button color="primary" (click)="openEdit(c)" aria-label="Edit category" matTooltip="Edit Category">
                <mat-icon>edit</mat-icon>
              </button>
              <button mat-icon-button color="warn" (click)="deleteCategory(c)" aria-label="Delete category" matTooltip="Delete Category">
                <mat-icon>delete</mat-icon>
              </button>
            </div>
          </li>
        </ul>
      </mat-card>
    </div>
  `,
  styles: [`
    .page { padding: 24px; max-width: 1100px; margin: 0 auto; }
    .header { display: flex; align-items: center; justify-content: space-between; gap: 16px; flex-wrap: wrap; }
    h1 { margin: 0; font-size: 1.6rem; font-weight: 700; }
    .actions { display: flex; gap: 10px; flex-wrap: wrap; }
    .list-card { margin-top: 16px; padding: 12px; border-radius: 16px; }
    .loading, .empty { padding: 16px; color: rgba(0,0,0,.65); }
    :host-context(html[data-theme='dark']) .loading,
    :host-context(html[data-theme='dark']) .empty { color: rgba(255,255,255,.75); }
    .list { list-style: none; margin: 0; padding: 0; }
    .list li { display: flex; align-items: center; justify-content: space-between; padding: 10px 6px; border-bottom: 1px solid rgba(0,0,0,.08); }
    :host-context(html[data-theme='dark']) .list li { border-bottom-color: rgba(255,255,255,.12); }
    .name { font-weight: 600; }
  `]
})
export class CategoriesPageComponent implements OnInit {
  categories: Category[] = [];
  loading = false;

  constructor(
    private categoryService: CategoryService,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.loading = true;
    this.categoryService.getAll().subscribe({
      next: (c) => {
        this.categories = c;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  openAdd(): void {
    const ref = this.dialog.open(AddCategoryDialogComponent, { width: '360px' });
    ref.afterClosed().subscribe((changed: boolean) => {
      if (changed) this.refresh();
    });
  }

  openEdit(cat: Category): void {
    const ref = this.dialog.open(AddCategoryDialogComponent, { width: '360px', data: { category: cat } });
    ref.afterClosed().subscribe((changed: boolean) => {
      if (changed) this.refresh();
    });
  }

  openManage(): void {
    const ref = this.dialog.open(ManageCategoriesDialogComponent, { width: '420px', data: { categories: this.categories } });
    ref.afterClosed().subscribe((changed: boolean) => {
      if (changed) this.refresh();
    });
  }

  deleteCategory(cat: Category): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      width: '420px',
      data: {
        title: 'Delete category',
        icon: 'delete',
        tone: 'danger',
        confirmLabel: 'Delete',
        message: `Are you sure you want to delete "${cat.name}"?`
      }
    });
    ref.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) return;
      this.categoryService.delete(cat.id).subscribe({
        next: () => this.refresh(),
        error: () => {} // Error handled globally or via toast
      });
    });
  }
}

