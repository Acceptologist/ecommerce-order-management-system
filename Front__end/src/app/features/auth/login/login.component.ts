import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({ selector: 'app-login', standalone: false, templateUrl: './login.component.html' })
export class LoginComponent implements OnInit {
  form = this.fb.group({
    username: ['', [Validators.required, Validators.minLength(3)]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });
  loading = false;
  error = '';
  hidePassword = true;
  returnUrl: string = '/products';

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    // Get the return url from route parameters or default to '/products'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/products';
  }

  submit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.error = '';
    this.auth.login(this.form.value as any).subscribe({
      next: () => {
        this.loading = false;
        this.notificationService.pushToast({
          id: Date.now(),
          userId: 0,
          message: 'Login successful!',
          type: 'Success',
          isRead: false,
          createdAt: new Date().toISOString()
        });
        this.router.navigateByUrl(this.returnUrl);
      },
      error: (e) => {
        const errorMsg = e.error?.error ||
          (Array.isArray(e.error?.errors) ? e.error.errors.join(', ') : undefined) ||
          'Login failed';
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
