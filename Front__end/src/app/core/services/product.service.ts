import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Product, PagedResult, CreateProductDto, UpdateProductDto } from '../models/product.model';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private base = `${environment.apiUrl}/products`;

  constructor(private http: HttpClient) {}

  getProducts(page = 1, pageSize = 10, search?: string, categoryId?: number, sortBy?: string, desc = false): Observable<PagedResult<Product>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search)     params = params.set('search', search);
    if (categoryId) params = params.set('categoryId', categoryId);
    if (sortBy)     params = params.set('sortBy', sortBy);
    if (desc)       params = params.set('desc', true);
    return this.http.get<PagedResult<Product>>(this.base, { params });
  }

  getById(id: number): Observable<Product> { return this.http.get<Product>(`${this.base}/${id}`); }
  create(dto: CreateProductDto): Observable<Product> { return this.http.post<Product>(this.base, dto); }
  update(id: number, dto: UpdateProductDto): Observable<void> { return this.http.put<void>(`${this.base}/${id}`, dto); }
  delete(id: number): Observable<void> { return this.http.delete<void>(`${this.base}/${id}`); }
}
