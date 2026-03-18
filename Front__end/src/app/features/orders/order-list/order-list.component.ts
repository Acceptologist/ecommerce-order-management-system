import { Component, OnDestroy, OnInit, ChangeDetectorRef, ViewChild, AfterViewInit } from '@angular/core';
import { Sort } from '@angular/material/sort';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Subscription, merge, of, BehaviorSubject } from 'rxjs';
import { switchMap, catchError } from 'rxjs/operators';
import { OrderService, PagedOrderResult } from '../../../core/services/order.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { OrderResponse } from '../../../core/models/order.model';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog.component';

@Component({ selector: 'app-order-list', standalone: false, templateUrl: './order-list.component.html', styleUrls: ['./order-list.component.scss'] })
export class OrderListComponent implements OnInit, OnDestroy, AfterViewInit {
  dataSource = new MatTableDataSource<OrderResponse>([]);
  
  // Filtering state
  searchQuery: string = '';
  filterStatus: string = '';
  startDate: Date | null = null;
  endDate: Date | null = null;
  
  // Sorting state
  sortBy: string = 'date';
  desc: boolean = true;

  totalCount = 0;
  loading = true;
  displayedColumns: string[] = ['id', 'date', 'items', 'shipTo', 'total', 'status'];
  private pollSub?: Subscription;
  private reload$ = new BehaviorSubject<void>(undefined);

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(
    private orderService: OrderService,
    private auth: AuthService,
    private notificationService: NotificationService,
    private cdr: ChangeDetectorRef,
    private dialog: MatDialog
  ) {}

  get isAdmin(): boolean { return this.auth.isAdmin; }

  ngOnInit(): void {
    if (this.isAdmin) this.displayedColumns = ['id', 'customer', 'date', 'items', 'shipTo', 'total', 'status', 'actions'];
    else this.displayedColumns = ['id', 'date', 'items', 'shipTo', 'total', 'status'];
  }

  ngAfterViewInit() {
    // Merge events that should trigger a reload
    this.pollSub = merge(
      this.reload$,
      this.notificationService.latestToast$ // Reload whenever a new notification arrives
    ).pipe(
      switchMap(() => {
        // Only show loading spinner on manual reloads, not background polling
        // to prevent UI flicker
        const isPolling = !this.loading && this.dataSource.data.length > 0;
        if (!isPolling) {
          this.loading = true;
          this.cdr.detectChanges();
        }
        
        const page = (this.paginator?.pageIndex || 0) + 1;
        const pageSize = this.paginator?.pageSize || 10;
        
        const request$ = this.isAdmin 
          ? this.orderService.getAllOrders(page, pageSize, this.searchQuery, this.filterStatus, this.startDate, this.endDate, this.sortBy, this.desc)
          : this.orderService.getOrders(page, pageSize, this.searchQuery, this.filterStatus, this.startDate, this.endDate, this.sortBy, this.desc);
          
        return request$.pipe(catchError(() => of(null)));
      })
    ).subscribe((res: PagedOrderResult | null) => {
      this.loading = false;
      if (res) {
        this.dataSource.data = res.items || [];
        this.totalCount = res.totalCount || 0;
      }
      this.cdr.detectChanges();
    });
  }

  onPageChange() {
    this.reload$.next();
  }

  ngOnDestroy(): void {
    if (this.pollSub) this.pollSub.unsubscribe();
  }

  canCancel(order: OrderResponse): boolean {
    const s = (order.status || '').toLowerCase();
    return s === 'pending' || s === 'processing';
  }

  cancelOrder(order: OrderResponse, event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    if (!this.canCancel(order)) return;
    const ref = this.dialog.open(ConfirmDialogComponent, {
      width: '420px',
      data: {
        title: 'Cancel order',
        icon: 'cancel',
        tone: 'danger',
        confirmLabel: 'Cancel order',
        message: `Cancel order #${order.id}? This cannot be undone.`
      }
    });
    ref.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) return;
      this.orderService.cancelOrder(order.id).subscribe({
        next: () => this.reload$.next(),
        error: () => {}
      });
    });
  }

  applyFilters(): void {
    if (this.paginator && this.paginator.pageIndex !== 0) {
      this.paginator.firstPage(); // This will trigger (page) event and reload
    } else {
      this.reload$.next();
    }
  }

  onSearchChange(value: string): void {
    this.searchQuery = value;
    this.applyFilters();
  }

  onStatusFilterChange(value: string): void {
    this.filterStatus = value;
    this.applyFilters();
  }

  clearFilters(): void {
    this.searchQuery = '';
    this.filterStatus = '';
    this.startDate = null;
    this.endDate = null;
    this.applyFilters();
  }

  sortChange(event: Sort) {
    if (!event.direction) {
      this.sortBy = 'date';
      this.desc = true;
    } else {
      this.sortBy = event.active;
      this.desc = event.direction === 'desc';
    }
    this.applyFilters();
  }

  statusColor(status: string) {
    switch(status.toLowerCase()) {
      case 'pending': return 'warn';
      case 'processing': return 'primary';
      case 'shipped': return 'accent';
      case 'completed': return 'primary';
      case 'cancelled': return 'accent';
      default: return '';
    }
  }
}
