import { Component, OnInit, AfterViewInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Router, NavigationEnd } from '@angular/router';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { finalize, filter } from 'rxjs/operators';
import { MatTableDataSource } from '@angular/material/table';
import { MatSort } from '@angular/material/sort';
import { MatPaginator } from '@angular/material/paginator';
import { ProductService } from '../../../core/services/product.service';
import { Product } from '../../../core/models/product.model';
import { AuthService } from '../../../core/services/auth.service';
import { CartService } from '../../../core/services/cart.service';
import { CategoryService, Category } from '../../../core/services/category.service';
import { NotificationService } from '../../../core/services/notification.service';
import { MatDialog } from '@angular/material/dialog';
import { AddCategoryDialogComponent } from '../../categories/add-category-dialog/add-category-dialog.component';
import { ManageCategoriesDialogComponent } from '../../categories/manage-categories-dialog/manage-categories-dialog.component';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-product-list',
  standalone: false,
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss']
})
export class ProductListComponent implements OnInit, AfterViewInit {
  products: Product[] = [];
  dataSource = new MatTableDataSource<Product>([]);
  displayedColumns: string[] = ['image', 'name', 'category', 'price', 'stock', 'actions'];
  totalCount = 0;
  categories: Category[] = [];
  page = 1;
  pageSize = 10;
  sortBy = '';
  desc = false;
  selectedCategory: number | null = null;
  searchCtrl = new FormControl('');
  loading = false;
  viewMode: 'table' | 'grid' = 'grid';

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(
    public productService: ProductService,
    public auth: AuthService,
    public cart: CartService,
    private categoryService: CategoryService,
    private notificationService: NotificationService,
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.categoryService.getAll().subscribe(c => this.categories = c);
    this.readQueryParams();
    this.page = 1;
    this.load();
    this.searchCtrl.valueChanges
      .pipe(debounceTime(400), distinctUntilChanged())
      .subscribe(() => {
        this.page = 1;
        this.load();
      });
    this.router.events
      .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd))
      .subscribe((e) => {
        if (e.urlAfterRedirects === '/products' || e.urlAfterRedirects.startsWith('/products?')) {
          this.readQueryParams();
          this.page = 1;
          this.load();
        }
      });
    this.route.queryParams.subscribe((params) => {
      this.selectedCategory = params['categoryId'] ? +params['categoryId'] : null;
      this.page = 1;
      this.load();
    });
    this.notificationService.stockUpdated$
      .pipe(filter((u): u is { productId: number; newStockQuantity: number } => u != null))
      .subscribe((u) => {
        const p = this.products.find((x) => x.id === u.productId);
        if (p) {
          p.stockQuantity = u.newStockQuantity;
          this.dataSource.data = [...this.products];
          this.dataSource._updateChangeSubscription();
          this.cdr.detectChanges();
        }
      });
  }

  private readQueryParams(): void {
    const q = this.route.snapshot.queryParamMap;
    const cat = q.get('categoryId');
    this.selectedCategory = cat ? +cat : null;
  }

  ngAfterViewInit(): void {}

  onCategoryChange(): void {
    this.page = 1;
    this.load();
  }

  onSortChange(event: { active: string; direction: 'asc' | 'desc' | '' }): void {
    const active = event.active;
    if (!event.direction || (active !== 'name' && active !== 'price' && active !== 'stock')) {
      this.sortBy = '';
      this.desc = false;
    } else {
      this.sortBy = active;
      this.desc = event.direction === 'desc';
    }
    this.page = 1;
    this.load();
  }

  load(): void {
    this.loading = true;
    this.productService.getProducts(
      this.page,
      this.pageSize,
      this.searchCtrl.value || undefined,
      this.selectedCategory || undefined,
      this.sortBy || undefined,
      this.desc
    ).pipe(
      finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (r) => {
        this.products = r.items;
        this.totalCount = r.totalCount;
        this.dataSource.data = r.items;
        
        // Ensure paginator stays in sync with current page state
        if (this.paginator) {
          this.paginator.pageIndex = this.page - 1;
          this.paginator.length = this.totalCount;
        }
        
        this.dataSource._updateChangeSubscription();
      },
      error: () => {
        // loading false is handled by finalize
      }
    });
  }

  onPageChange(e: any): void {
    this.page = e.pageIndex + 1;
    this.pageSize = e.pageSize;
    this.load();
  }

  onSort(field: string): void {
    this.desc = this.sortBy === field ? !this.desc : false;
    this.sortBy = field;
    this.load();
  }

  toggleSortDirection(): void {
    this.desc = !this.desc;
  }

  addToCart(p: Product): void {
    if (p.stockQuantity > 0) {
      this.cart.addItem(p, 1);
      this.notificationService.pushToast({
        id: Date.now(),
        userId: 0,
        message: `1 × ${p.name} added to cart`,
        type: 'Success',
        isRead: false,
        createdAt: new Date().toISOString()
      });
    }
  }

  toggleView(mode: 'table' | 'grid'): void {
    this.viewMode = mode;
  }

  getStockClass(quantity: number): string {
    if (quantity === 0) {
      return 'out-of-stock';
    } else if (quantity < 5) {
      return 'low-stock';
    }
    return 'in-stock';
  }

  getProductImage(product: Product): string {
    return product.imageUrl?.trim() || 'assets/images/placeholder.png';
  }

  openAddCategoryDialog(): void {
    const ref = this.dialog.open(AddCategoryDialogComponent, { width: '360px' });
    ref.afterClosed().subscribe((added: boolean) => {
      if (added) {
        this.categoryService.getAll().subscribe((c) => {
          this.categories = c;
          this.cdr.detectChanges();
        });
      }
    });
  }

  openManageCategoriesDialog(): void {
    const ref = this.dialog.open(ManageCategoriesDialogComponent, { width: '420px', data: { categories: this.categories } });
    ref.afterClosed().subscribe((refreshed: boolean) => {
      if (refreshed) {
        this.categoryService.getAll().subscribe((c) => {
          this.categories = c;
          this.cdr.detectChanges();
        });
        this.load();
      }
    });
  }

  deleteProduct(p: Product): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      width: '420px',
      data: {
        title: 'Delete product',
        icon: 'delete',
        tone: 'danger',
        confirmLabel: 'Delete',
        message: `Soft-delete "${p.name}"? It will be hidden from the catalog.`
      }
    });
    ref.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) return;
      this.productService.delete(p.id).subscribe({
        next: () => {
          this.notificationService.pushToast({
            id: Date.now(),
            userId: 0,
            message: 'Product deleted.',
            type: 'Success',
            isRead: false,
            createdAt: new Date().toISOString()
          });
          this.load();
        },
        error: (err) => {
          this.notificationService.pushToast({
            id: Date.now(),
            userId: 0,
            message: err?.error?.message || 'Failed to delete product.',
            type: 'Error',
            isRead: false,
            createdAt: new Date().toISOString()
          });
        }
      });
    });
  }
}
