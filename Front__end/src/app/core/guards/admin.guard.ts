import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

@Injectable({ providedIn: 'root' })
export class AdminGuard implements CanActivate {
  constructor(
    private auth: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  canActivate(): boolean {
    if (this.auth.isAdmin) return true;
    this.notificationService.pushToast({
      id: Date.now(),
      userId: 0,
      message: 'You do not have permission to access the dashboard.',
      type: 'Warning',
      isRead: false,
      createdAt: new Date().toISOString()
    });
    this.router.navigate(['/products']);
    return false;
  }
}
