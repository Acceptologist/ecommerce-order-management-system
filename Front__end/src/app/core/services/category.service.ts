import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Category { id: number; name: string; }

@Injectable({ providedIn: 'root' })
export class CategoryService {
  constructor(private http: HttpClient) {}
  getAll(): Observable<Category[]> { return this.http.get<Category[]>(`${environment.apiUrl}/categories`); }
  create(name: string): Observable<Category> {
    return this.http.post<Category>(`${environment.apiUrl}/categories`, { name });
  }
  update(id: number, name: string): Observable<Category> {
    return this.http.put<Category>(`${environment.apiUrl}/categories/${id}`, { name });
  }
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/categories/${id}`);
  }
}
