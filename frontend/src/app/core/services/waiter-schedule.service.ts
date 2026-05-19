import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { WaiterScheduleResponse, CreateWaiterScheduleRequest, UpdateWaiterScheduleRequest } from '../models/schedule.models';

@Injectable({ providedIn: 'root' })
export class WaiterScheduleService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiBaseUrl}`;

  getAll(waiterId: string): Observable<WaiterScheduleResponse[]> {
    return this.http.get<WaiterScheduleResponse[]>(`${this.apiUrl}/waiters/${waiterId}/schedule`);
  }

  getByDate(waiterId: string, date: string): Observable<WaiterScheduleResponse> {
    return this.http.get<WaiterScheduleResponse>(`${this.apiUrl}/waiters/${waiterId}/schedule/${date}`);
  }

  create(waiterId: string, req: CreateWaiterScheduleRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/waiters/${waiterId}/schedule`, req);
  }

  update(waiterId: string, scheduleId: string, req: UpdateWaiterScheduleRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/waiters/${waiterId}/schedule/${scheduleId}`, req);
  }

  delete(waiterId: string, scheduleId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/waiters/${waiterId}/schedule/${scheduleId}`);
  }
}

