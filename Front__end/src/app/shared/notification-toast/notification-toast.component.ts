import { Component, OnInit, OnDestroy } from '@angular/core';
import { animate, style, transition, trigger } from '@angular/animations';
import { NotificationService } from '../../core/services/notification.service';
import { Notification } from '../../core/models/notification.model';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-notification-toast',
  standalone: false,
  templateUrl: './notification-toast.component.html',
  styleUrls: ['./notification-toast.component.scss'],
  animations: [
    trigger('slideInOut', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(100%)' }),
        animate('220ms ease-out', style({ opacity: 1, transform: 'translateX(0)' }))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ opacity: 0, transform: 'translateX(100%)' }))
      ])
    ])
  ]
})
export class NotificationToastComponent implements OnInit, OnDestroy {
  toasts: Notification[] = [];
  private subscription?: Subscription;
  private autoHideTimeouts: Map<number, any> = new Map();

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.subscription = this.notificationService.latestToast$.subscribe((notification) => {
      if (!notification) return;

      this.notificationService.playNotificationEffects(notification);
      this.notificationService.showBrowserNotificationIfAllowed(notification);
      this.toasts.push(notification);

      // Auto-hide after 7 seconds
      const timeout = setTimeout(() => {
        this.dismiss(notification.id);
      }, 7000);

      this.autoHideTimeouts.set(notification.id, timeout);
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
    // Clear all pending timeouts
    this.autoHideTimeouts.forEach((timeout) => clearTimeout(timeout));
    this.autoHideTimeouts.clear();
  }

  dismiss(id: number): void {
    const timeout = this.autoHideTimeouts.get(id);
    if (timeout) {
      clearTimeout(timeout);
      this.autoHideTimeouts.delete(id);
    }
    this.toasts = this.toasts.filter((t) => t.id !== id);
  }

  dismissAll(): void {
    this.autoHideTimeouts.forEach((timeout) => clearTimeout(timeout));
    this.autoHideTimeouts.clear();
    this.toasts = [];
  }

  getToastIcon(type: string): string {
    const iconMap: Record<string, string> = {
      Success: 'check_circle',
      Error: 'error',
      Warning: 'warning',
      Info: 'info_outlined'
    };
    return iconMap[type] ?? 'notifications';
  }

  trackById(_: number, toast: Notification): number {
    return toast.id;
  }
}
