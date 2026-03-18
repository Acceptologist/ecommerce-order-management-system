import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { AdminGuard } from './core/guards/admin.guard';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { ProductListComponent } from './features/products/product-list/product-list.component';
import { ProductDetailComponent } from './features/products/product-detail/product-detail.component';
import { ProductFormComponent } from './features/products/product-form/product-form.component';
import { CartComponent } from './features/cart/cart.component';
import { CheckoutComponent } from './features/checkout/checkout.component';
import { OrderSummaryComponent } from './features/orders/order-summary/order-summary.component';
import { OrderListComponent } from './features/orders/order-list/order-list.component';
import { NotificationCenterComponent } from './features/notifications/notification-center/notification-center.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { CategoriesPageComponent } from './features/categories/categories-page/categories-page.component';

const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'products' },
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard, AdminGuard] },
  { path: 'login',    component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'products', component: ProductListComponent },
  { path: 'products/new',  component: ProductFormComponent, canActivate: [AuthGuard, AdminGuard] },
  { path: 'products/:id/edit', component: ProductFormComponent, canActivate: [AuthGuard, AdminGuard] },
  { path: 'products/:id',  component: ProductDetailComponent },
  { path: 'cart',      component: CartComponent }, // Removed AuthGuard so anyone can view cart
  { path: 'checkout',  component: CheckoutComponent, canActivate: [AuthGuard] },
  { path: 'orders',    component: OrderListComponent, canActivate: [AuthGuard] },
  { path: 'orders/:id', component: OrderSummaryComponent, canActivate: [AuthGuard] },
  { path: 'categories', component: CategoriesPageComponent, canActivate: [AuthGuard, AdminGuard] },
  { path: 'notifications', component: NotificationCenterComponent, canActivate: [AuthGuard] },
  { path: '**', redirectTo: '/products' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
