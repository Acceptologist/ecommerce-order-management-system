import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { NotificationService } from '../../../core/services/notification.service';
import { Notification } from '../../../core/models/notification.model';

@Component({
  selector: 'app-notification-center',
  standalone: false,
  templateUrl: './notification-center.component.html',
  styleUrls: ['./notification-center.component.scss']
})
export class NotificationCenterComponent implements OnInit, OnDestroy {
  notifications: Notification[] = [];
  filteredNotifications: Notification[] = [];
  loading = false;
  unreadCount = 0;
  totalCount = 0;
  page = 1;
  pageSize = 10;
  filterType: string = '';
  private sub?: Subscription;

  constructor(
    public notificationService: NotificationService,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.sub = this.notificationService.notifications$.subscribe(notifications => {
      this.notifications = notifications;
      this.totalCount = notifications.length;
      this.applyFilter();
      this.updateUnreadCount();
    });
    this.load();
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  applyFilter(): void {
    if (!this.filterType) {
      this.filteredNotifications = this.notifications;
    } else {
      this.filteredNotifications = this.notifications.filter((n) => n.type === this.filterType);
    }
    this.cdr.detectChanges();
  }

  load(): void {
    this.loading = true;
    this.notificationService.getAll().pipe(
      finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (notifications) => {
        // Update the service subject so it broadcasts to our subscription
        (this.notificationService as any).notificationsSubject.next(notifications);
      },
      error: () => {
        // loading is handled in finalize
      }
    });
  }

  private updateUnreadCount(): void {
    this.unreadCount = this.notifications.filter((n) => !n.isRead).length;
    this.notificationService.setUnreadCount(this.unreadCount);
  }

  markAsRead(id: number): void {
    this.notificationService.markAsRead(id).subscribe(() => {
      const notification = this.notifications.find((n) => n.id === id);
      if (notification) {
        notification.isRead = true;
        this.updateUnreadCount();
      }
    });
  }

  toggleRead(id: number): void {
    const notification = this.notifications.find((n) => n.id === id);
    if (notification && !notification.isRead) {
      this.markAsRead(id);
    }
  }

  /** Navigate to order page when notification is order-related, else products; mark as read. */
  onNotificationClick(notification: Notification): void {
    if (!notification.isRead) {
      this.markAsRead(notification.id);
    }
    if (notification.orderId != null) {
      this.router.navigate(['/orders', notification.orderId]);
    } else {
      this.router.navigate(['/products']);
    }
  }

  markAllAsRead(): void {
    this.notifications.forEach((n) => {
      if (!n.isRead) {
        this.markAsRead(n.id);
      }
    });
  }

  delete(id: number): void {
    // If there's a delete endpoint, implement it here
    // For now, just filter from the list
    this.notifications = this.notifications.filter((n) => n.id !== id);
    this.applyFilter();
    this.totalCount = this.notifications.length;
    this.updateUnreadCount();
  }

  onPageChange(event: any): void {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    // Implement pagination in backend if needed
    this.load();
  }

  getIconFor(type: string): string {
    const iconMap: Record<string, string> = {
      Success: 'check_circle',
      Error: 'error',
      Warning: 'warning',
      Info: 'info_outlined'
    };
    return iconMap[type] ?? 'notifications';
  }

  getNotificationClasses(notification: Notification): Record<string, boolean> {
    return {
      'unread': !notification.isRead,
      [`notification-${notification.type.toLowerCase()}`]: true
    };
  }

  trackById(_: number, notification: Notification): number {
    return notification.id;
  }
}
