import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ReservationResponse,
  CreateReservationRequest,
  UpdateReservationStatusRequest,
  UpdateReservationTimeRequest,
  UpdateReservationTableRequest,
  UpdateReservationAssignedWaiterRequest,
} from '../models/reservation.models';

@Injectable({ providedIn: 'root' })
export class ReservationsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/reservations`;

  getAll(filters?: { restaurantId?: string; date?: string }): Observable<ReservationResponse[]> {
    let params = new HttpParams();
    if (filters?.restaurantId) params = params.set('restaurantId', filters.restaurantId);
    if (filters?.date) params = params.set('date', filters.date);
    return this.http.get<ReservationResponse[]>(this.baseUrl, { params });
  }

  create(req: CreateReservationRequest): Observable<ReservationResponse> {
    return this.http.post<ReservationResponse>(this.baseUrl, req);
  }

  updateStatus(id: string, req: UpdateReservationStatusRequest): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/status`, req);
  }

  updateTime(id: string, req: UpdateReservationTimeRequest): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/time`, req);
  }

  updateTable(id: string, req: UpdateReservationTableRequest): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/table`, req);
  }

  assignWaiter(id: string, req: UpdateReservationAssignedWaiterRequest): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/waiter`, req);
  }

  cancel(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  getMy(): Observable<ReservationResponse[]> {
    return this.http.get<ReservationResponse[]>(this.baseUrl);
  }
}

