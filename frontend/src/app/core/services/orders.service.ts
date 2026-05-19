import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { OrderResponse, CreateOrderRequest, UpdateOrderStatusRequest } from '../models/order.models';

@Injectable({ providedIn: 'root' })
export class OrdersService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/orders`;

  getAll(filters?: { restaurantId?: string; status?: string }): Observable<OrderResponse[]> {
    let params = new HttpParams();
    if (filters?.restaurantId) params = params.set('restaurantId', filters.restaurantId);
    if (filters?.status) params = params.set('status', filters.status);
    return this.http.get<OrderResponse[]>(this.baseUrl, { params });
  }

  getById(id: string): Observable<OrderResponse> {
    return this.http.get<OrderResponse>(`${this.baseUrl}/${id}`);
  }

  create(reservationId: string, req: CreateOrderRequest): Observable<OrderResponse> {
    return this.http.post<OrderResponse>(`${environment.apiBaseUrl}/reservations/${reservationId}/orders`, req);
  }

  updateStatus(id: string, req: UpdateOrderStatusRequest): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/status`, req);
  }

  getMy(): Observable<OrderResponse[]> {
    return this.http.get<OrderResponse[]>(`${this.baseUrl}/my`);
  }
}

