import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { LoginRequest, RegisterRequest, TokenResponse } from '../models/auth.model';
import { NotificationService } from './notification.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly TOKEN_KEY   = environment.tokenKey;
  private readonly REFRESH_KEY = environment.refreshKey;
  private readonly USER_KEY    = environment.userKey;

  private currentUserSubject = new BehaviorSubject<TokenResponse | null>(this.loadUser());
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient, private router: Router, private notificationService: NotificationService) {}

  get currentUser(): TokenResponse | null { return this.currentUserSubject.value; }
  get isLoggedIn(): boolean { return !!this.getAccessToken(); }
  get isAdmin(): boolean { return this.currentUser?.roles?.includes('Admin') ?? false; }
  getAccessToken(): string | null { return localStorage.getItem(this.TOKEN_KEY); }

  login(req: LoginRequest): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${environment.apiUrl}/auth/login`, req)
      .pipe(tap(r => this.storeTokens(r)));
  }

  register(req: RegisterRequest): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${environment.apiUrl}/auth/register`, req)
      .pipe(tap(r => this.storeTokens(r)));
  }

  logout(): void {
    const token = this.getAccessToken();
    if (token) {
      this.http.post(`${environment.apiUrl}/auth/logout`, {}, { headers: { Authorization: `Bearer ${token}` } })
        .subscribe({ next: () => {}, error: () => {} });
    }
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUserSubject.next(null);
    this.notificationService.stopSignalR();
    this.router.navigate(['/login']);
  }

  refreshToken(): Observable<TokenResponse> {
    const refreshToken = localStorage.getItem(this.REFRESH_KEY) ?? '';
    return this.http.post<TokenResponse>(`${environment.apiUrl}/auth/refresh`, { refreshToken })
      .pipe(tap(r => this.storeTokens(r)));
  }

  private storeTokens(r: TokenResponse): void {
    localStorage.setItem(this.TOKEN_KEY,   r.accessToken);
    localStorage.setItem(this.REFRESH_KEY, r.refreshToken);
    localStorage.setItem(this.USER_KEY,    JSON.stringify(r));
    this.currentUserSubject.next(r);
    // Start SignalR connection after login
    this.notificationService.startSignalR(r.accessToken);
  }

  private loadUser(): TokenResponse | null {
    const raw = localStorage.getItem(this.USER_KEY);
    return raw ? JSON.parse(raw) : null;
  }
}
