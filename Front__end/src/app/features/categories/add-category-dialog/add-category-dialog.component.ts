import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CategoryService, Category } from '../../../core/services/category.service';

export interface AddCategoryDialogData {
  category?: Category | null;
}

@Component({
  selector: 'app-add-category-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>category</mat-icon>
      {{ isEdit ? 'Edit Category' : 'Add Category' }}
    </h2>
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Category name</mat-label>
          <input matInput formControlName="name" placeholder="e.g. Electronics" />
          <mat-error *ngIf="form.get('name')?.hasError('required')">Name is required</mat-error>
          <mat-error *ngIf="form.get('name')?.hasError('minlength')">At least 2 characters</mat-error>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button color="primary" (click)="submit()" [disabled]="form.invalid || saving">
        {{ saving ? (isEdit ? 'Saving...' : 'Adding...') : (isEdit ? 'Save' : 'Add') }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    h2 { display: flex; align-items: center; gap: 10px; }
    .full-width { width: 100%; }
    mat-dialog-content { min-width: 280px; padding-top: 18px; }
  `]
})
export class AddCategoryDialogComponent {
  form: FormGroup;
  saving = false;
  isEdit = false;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<AddCategoryDialogComponent>,
    private categoryService: CategoryService,
    @Inject(MAT_DIALOG_DATA) public data: AddCategoryDialogData | null
  ) {
    const category = data?.category;
    this.isEdit = !!category;
    this.form = this.fb.group({
      name: [category?.name ?? '', [Validators.required, Validators.minLength(2)]]
    });
  }

  submit(): void {
    if (this.form.invalid || this.saving) return;
    this.saving = true;
    const name = this.form.value.name.trim();
    if (this.isEdit && this.data?.category) {
      this.categoryService.update(this.data.category.id, name).subscribe({
        next: () => this.dialogRef.close(true),
        error: () => this.saving = false
      });
    } else {
      this.categoryService.create(name).subscribe({
        next: () => this.dialogRef.close(true),
        error: () => this.saving = false
      });
    }
  }
}

