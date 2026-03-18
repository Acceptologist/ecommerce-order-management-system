import { Component, OnInit, EventEmitter, Output, Input } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { CartService } from '../../core/services/cart.service';
import { NotificationService } from '../../core/services/notification.service';
import { ThemeService } from '../../core/services/theme.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-navbar',
  standalone: false,
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit {
  @Output() menuToggle = new EventEmitter<void>();
  @Output() themeToggle = new EventEmitter<void>();
  @Input() isDark = false;

  unreadCount$: Observable<number>;

  constructor(
    public auth: AuthService,
    public cart: CartService,
    public notif: NotificationService,
    public themeService: ThemeService
  ) {
    this.unreadCount$ = this.notif.unreadCount$;
  }

  ngOnInit(): void {
    this.isDark = this.themeService.isDark;
    
    if (this.auth.isLoggedIn) {
      this.notif.getUnreadCount().subscribe(r => this.notif.setUnreadCount(r.count));
      const token = this.auth.getAccessToken()!;
      this.notif.startSignalR(token);
    }

    this.themeService.theme$.subscribe(theme => {
      this.isDark = theme === 'dark';
    });
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  logout(): void {
    this.auth.logout();
    this.notif.stopSignalR();
  }
}
