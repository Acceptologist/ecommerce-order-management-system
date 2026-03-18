import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateOrderRequest, OrderResponse, CartValidationResult, OrderItemRequest } from '../models/order.model';

export interface PagedOrderResult {
  page: number;
  pageSize: number;
  totalCount: number;
  items: OrderResponse[];
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private base = `${environment.apiUrl}/orders`;
  constructor(private http: HttpClient) {}

  validateCart(items: OrderItemRequest[]): Observable<CartValidationResult> {
    return this.http.post<CartValidationResult>(`${this.base}/validate-cart`, { items });
  }
  createOrder(req: CreateOrderRequest): Observable<OrderResponse> { return this.http.post<OrderResponse>(this.base, req); }
  
  getOrders(
    page: number = 1,
    pageSize: number = 10,
    search?: string,
    status?: string,
    startDate?: Date | null,
    endDate?: Date | null,
    sortBy?: string,
    desc: boolean = true
  ): Observable<PagedOrderResult> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString())
      .set('desc', desc.toString());

    if (search) params = params.set('search', search);
    if (status) params = params.set('status', status);
    if (startDate) params = params.set('startDate', startDate.toISOString());
    if (endDate) params = params.set('endDate', endDate.toISOString());
    if (sortBy) params = params.set('sortBy', sortBy);

    return this.http.get<PagedOrderResult>(this.base, { params });
  }

  getAllOrders(
    page: number = 1,
    pageSize: number = 10,
    search?: string,
    status?: string,
    startDate?: Date | null,
    endDate?: Date | null,
    sortBy?: string,
    desc: boolean = true
  ): Observable<PagedOrderResult> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString())
      .set('desc', desc.toString());

    if (search) params = params.set('search', search);
    if (status) params = params.set('status', status);
    if (startDate) params = params.set('startDate', startDate.toISOString());
    if (endDate) params = params.set('endDate', endDate.toISOString());
    if (sortBy) params = params.set('sortBy', sortBy);

    return this.http.get<PagedOrderResult>(`${this.base}/all`, { params });
  }

  getById(id: number): Observable<OrderResponse> { return this.http.get<OrderResponse>(`${this.base}/${id}`); }
  cancelOrder(id: number): Observable<void> { return this.http.post<void>(`${this.base}/${id}/cancel`, {}); }
}
