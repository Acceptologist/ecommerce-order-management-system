import { Component, OnInit } from '@angular/core';
import { OverlayContainer } from '@angular/cdk/overlay';
import { AuthService } from './core/services/auth.service';
import { ThemeService } from './core/services/theme.service';
import { LoadingService } from './core/services/loading.service';

@Component({
  selector: 'app-root',
  standalone: false,
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  isDark = false;

  constructor(
    private overlay: OverlayContainer,
    public auth: AuthService,
    public themeService: ThemeService,
    public loadingService: LoadingService
  ) {}

  ngOnInit(): void {
    this.isDark = this.themeService.isDark;
    this.applyTheme();
    this.themeService.theme$.subscribe(theme => {
      this.isDark = theme === 'dark';
      this.applyTheme();
    });
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  private applyTheme(): void {
    const classList = this.overlay.getContainerElement().classList;
    if (this.isDark) {
      classList.add('dark-theme');
    } else {
      classList.remove('dark-theme');
    }
  }
}
