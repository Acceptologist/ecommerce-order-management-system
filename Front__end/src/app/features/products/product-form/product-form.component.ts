import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';
import { CategoryService, Category } from '../../../core/services/category.service';

@Component({ selector: 'app-product-form', standalone: false, templateUrl: './product-form.component.html', styleUrls: ['./product-form.component.scss'] })
export class ProductFormComponent implements OnInit {
  form = this.fb.group({
    name:          ['', [Validators.required, Validators.maxLength(200)]],
    description:   ['', Validators.required],
    imageUrl:      ['', [Validators.maxLength(500), Validators.pattern('https?://.+')]],
    price:         [0,  [Validators.required, Validators.min(0.01)]],
    discountRate:  [0,  [Validators.min(0), Validators.max(100)]],
    stockQuantity: [0,  [Validators.required, Validators.min(0)]],
    categoryId:    [0,  Validators.required],
  });
  categories: Category[] = [];
  isEdit = false;
  productId?: number;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private categoryService: CategoryService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.categoryService.getAll().subscribe(c => this.categories = c);
    this.productId = Number(this.route.snapshot.paramMap.get('id')) || undefined;
    if (this.productId) {
      this.isEdit = true;
      this.productService.getById(this.productId).subscribe(p => this.form.patchValue(p));
    }
  }

  get imageUrlControl() {
    return this.form.get('imageUrl');
  }

  onImageError(event: Event): void {
    (event.target as HTMLImageElement).style.display = 'none';
  }

  submit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    const dto = this.form.value as any;
    const action: import('rxjs').Observable<any> = this.isEdit
      ? this.productService.update(this.productId!, dto)
      : this.productService.create(dto);
    action.subscribe({
      next: () => this.router.navigate(['/products']),
      error: () => this.loading = false
    });
  }
}
