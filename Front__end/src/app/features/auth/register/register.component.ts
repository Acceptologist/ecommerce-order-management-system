import { Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({ selector: 'app-register', standalone: false, templateUrl: './register.component.html' })
export class RegisterComponent {
  form = this.fb.group({
    username: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });
  loading = false;
  error = '';
  hidePassword = true;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  get passwordStrength(): number {
    const val = this.form.get('password')?.value || '';
    let score = 0;
    if (val.length >= 6) score += 25;
    if (val.length >= 10) score += 15;
    if (/[A-Z]/.test(val)) score += 20;
    if (/[0-9]/.test(val)) score += 20;
    if (/[^A-Za-z0-9]/.test(val)) score += 20;
    return Math.min(score, 100);
  }

  get passwordStrengthClass(): string {
    const s = this.passwordStrength;
    if (s < 40) return 'weak';
    if (s < 70) return 'medium';
    return 'strong';
  }

  get passwordStrengthLabel(): string {
    const s = this.passwordStrength;
    if (s < 40) return 'Weak';
    if (s < 70) return 'Medium';
    return 'Strong';
  }

  submit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.error = '';
    this.auth.register(this.form.value as any).subscribe({
      next: () => {
        this.loading = false;
        this.notificationService.pushToast({
          id: Date.now(),
          userId: 0,
          message: 'Registration successful! Welcome!',
          type: 'Success',
          isRead: false,
          createdAt: new Date().toISOString()
        });
        this.router.navigate(['/products']);
      },
      error: (e) => {
        const errorMsg = e.error?.message || 'Registration failed';
        this.error = errorMsg;
        this.loading = false;
        this.notificationService.pushToast({
          id: Date.now(),
          userId: 0,
          message: errorMsg,
          type: 'Error',
          isRead: false,
          createdAt: new Date().toISOString()
        });
      }
    });
  }
}
