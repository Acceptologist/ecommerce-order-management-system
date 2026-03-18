import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { OrderService } from '../../../core/services/order.service';
import { OrderResponse } from '../../../core/models/order.model';
import { NotificationService } from '../../../core/services/notification.service';

@Component({ selector: 'app-order-summary', standalone: false, templateUrl: './order-summary.component.html', styleUrls: ['./order-summary.component.scss'] })
export class OrderSummaryComponent implements OnInit, OnDestroy {
  order?: OrderResponse;
  private notifSub?: Subscription;

  constructor(
    private route: ActivatedRoute, 
    private orderService: OrderService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) return;
    
    // Initial load
    this.loadOrder(id);

    // Listen to real-time notifications to update immediately
    this.notifSub = this.notificationService.latestToast$.pipe(
      filter(n => n != null && n.orderId === id)
    ).subscribe(() => {
      this.loadOrder(id);
    });
  }

  private loadOrder(id: number): void {
    this.orderService.getById(id).subscribe(o => {
      this.order = o;
    });
  }

  ngOnDestroy(): void {
    this.notifSub?.unsubscribe();
  }

  get isCancelled(): boolean {
    return (this.order?.status || '').toLowerCase() === 'cancelled';
  }

  get currentStep(): 1 | 2 | 3 | 4 {
    const s = (this.order?.status || '').toLowerCase();
    if (s === 'completed') return 4;
    if (s === 'shipped') return 3;
    if (s === 'processing') return 2;
    return 1;
  }

  isStepCompleted(step: 1 | 2 | 3 | 4): boolean {
    const s = (this.order?.status || '').toLowerCase();
    if (!s || s === 'cancelled') return false;
    if (s === 'completed') return true;
    if (s === 'shipped') return step <= 3;
    if (s === 'processing') return step <= 2;
    return step === 1; // pending/other
  }

  isStepCurrent(step: 1 | 2 | 3 | 4): boolean {
    return !this.isCancelled && this.currentStep === step;
  }

  statusColor(status: string | undefined): string {
    if (!status) { return ''; }
    switch (status.toLowerCase()) {
      case 'pending': return 'warn';
      case 'completed': return 'primary';
      case 'cancelled': return 'accent';
      default: return '';
    }
  }
}
