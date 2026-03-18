import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PaymentRequest, PaymentResult } from '../models/payment.model';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private base = `${environment.apiUrl}/payments`;

  constructor(private http: HttpClient) {}

  simulatePayment(request: PaymentRequest): Observable<PaymentResult> {
    return this.http.post<PaymentResult>(`${this.base}/simulate`, request);
  }
}
