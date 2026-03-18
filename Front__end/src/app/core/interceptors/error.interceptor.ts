import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';
import { AuthService } from '../services/auth.service';
import { Notification } from '../models/notification.model';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notif = inject(NotificationService);
  const auth = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401) {
        auth.logout();
        notif.pushToast({ id: Date.now(), userId: 0, message: 'Session expired. Please login again.', type: 'Error', isRead: false, createdAt: new Date().toISOString() } as Notification);
        router.navigate(['/login']);
        return throwError(() => err);
      }

      const message = err.error?.message || err.error?.errors?.[0] || err.message || 'An error occurred';
      notif.pushToast({ id: Date.now(), userId: 0, message, type: 'Error', isRead: false, createdAt: new Date().toISOString() } as Notification);
      return throwError(() => err);
    })
  );
};
