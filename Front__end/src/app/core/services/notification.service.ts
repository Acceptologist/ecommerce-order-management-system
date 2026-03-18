import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Notification } from '../models/notification.model';
import * as signalR from '@microsoft/signalr';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private base = `${environment.apiUrl}/notifications`;
  private readonly UNREAD_COUNT_KEY = environment.unreadCountKey;
  
  private unreadCountSubject = new BehaviorSubject<number>(this.loadUnreadCountFromStorage());
  unreadCount$ = this.unreadCountSubject.asObservable();

  private toastSubject = new BehaviorSubject<Notification | null>(null);
  latestToast$ = this.toastSubject.asObservable();

  /** Real-time stock updates from SignalR (productId, newStockQuantity). */
  private stockUpdatedSubject = new BehaviorSubject<{ productId: number; newStockQuantity: number } | null>(null);
  stockUpdated$ = this.stockUpdatedSubject.asObservable();

  /**
   * Publish a toast notification that will also be picked up by toast component.
   */
  pushToast(notification: Notification): void {
    this.toastSubject.next(notification);
  }

  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  notifications$ = this.notificationsSubject.asObservable();

  private hubConnection?: signalR.HubConnection;
  private lastReceivedNotificationId: number | null = null;

  constructor(private http: HttpClient) {
    // Load unread count from localStorage on service initialization
    this.loadUnreadCount();
  }

  startSignalR(token: string): void {
    if (this.hubConnection) {
      return; // Connection already active
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();

    // Real-time stock updates (e.g. after order deducts stock)
    this.hubConnection.on('StockUpdated', (payload: { productId: number; newStockQuantity: number }) => {
      this.stockUpdatedSubject.next(payload);
    });

    // Handle incoming notifications from SignalR
    this.hubConnection.on('ReceiveNotification', (n: Notification) => {
      // de-dupe: SignalR reconnects and backend retries can cause duplicates
      if (n?.id != null) {
        if (this.lastReceivedNotificationId === n.id) return;
        if (this.notificationsSubject.value.some(x => x.id === n.id)) return;
        this.lastReceivedNotificationId = n.id;
      }
      // Emit toast
      this.toastSubject.next(n);
      this.playNotificationEffects(n);
      this.showBrowserNotificationIfAllowed(n);
      // Update unread count
      const newCount = this.unreadCountSubject.value + 1;
      this.setUnreadCount(newCount);
      // Add to notifications list if subscribed
      const current = this.notificationsSubject.value;
      this.notificationsSubject.next([n, ...current]);
    });

    this.hubConnection.start().catch(err => {
      console.error('SignalR connection failed:', err);
    });
  }

  stopSignalR(): void {
    if (this.hubConnection) {
      this.hubConnection.stop().catch(console.error);
      this.hubConnection = undefined;
    }
  }

  getAll(): Observable<Notification[]> {
    return this.http.get<Notification[]>(this.base);
  }

  refreshNotifications(): void {
    this.getAll().subscribe({
      next: (list) => {
        this.notificationsSubject.next(list);
        const unread = list.filter((n) => !n.isRead).length;
        this.setUnreadCount(unread);
      },
      error: () => {
        // Ignore refresh errors to avoid breaking UX flows
      }
    });
  }

  getUnreadCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.base}/unread-count`);
  }

  markAsRead(id: number): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/read`, {});
  }

  setUnreadCount(count: number): void {
    this.unreadCountSubject.next(count);
    this.saveUnreadCountToStorage(count);
  }

  private loadUnreadCount(): void {
    // Try to load from API, fallback to localStorage
    this.getUnreadCount().subscribe({
      next: (response) => {
        this.setUnreadCount(response.count);
      },
      error: () => {
        // Use localStorage value as fallback
        const stored = this.loadUnreadCountFromStorage();
        this.unreadCountSubject.next(stored);
      }
    });
  }

  private loadUnreadCountFromStorage(): number {
    try {
      const stored = localStorage.getItem(this.UNREAD_COUNT_KEY);
      return stored ? parseInt(stored, 10) : 0;
    } catch {
      return 0;
    }
  }

  private saveUnreadCountToStorage(count: number): void {
    try {
      localStorage.setItem(this.UNREAD_COUNT_KEY, count.toString());
    } catch (error) {
      console.error('Failed to save unread count to localStorage:', error);
    }
  }

  /** Optional sound and vibration for new notifications. Call from toast component for any displayed toast. */
  playNotificationEffects(_n: Notification): void {
    try {
      if (typeof navigator !== 'undefined' && navigator.vibrate) {
        navigator.vibrate(200);
      }
    } catch {
      // Ignore
    }
  }

  /** Browser Notification API for persistence when tab is in background. */
  showBrowserNotificationIfAllowed(n: Notification): void {
    try {
      if (typeof Notification === 'undefined' || Notification.permission !== 'granted') return;
      new Notification('ShopHub', { body: n.message, icon: '/assets/icons/icon-192x192.png' });
    } catch {
      // Ignore
    }
  }

  /** Call once (e.g. after login) to request browser notification permission. */
  requestBrowserNotificationPermission(): void {
    try {
      if (typeof Notification !== 'undefined' && Notification.permission === 'default') {
        Notification.requestPermission();
      }
    } catch {
      // Ignore
    }
  }
}
