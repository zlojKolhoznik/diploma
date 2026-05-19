import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { WaiterResponse, AssignWaiterRoleRequest } from '../models/waiter.models';

@Injectable({ providedIn: 'root' })
export class WaitersService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/waiters`;

  getAll(): Observable<WaiterResponse[]> {
    return this.http.get<WaiterResponse[]>(this.baseUrl);
  }

  getById(id: string): Observable<WaiterResponse> {
    return this.http.get<WaiterResponse>(`${this.baseUrl}/${id}`);
  }

  create(userId: string, req: AssignWaiterRoleRequest): Observable<WaiterResponse> {
    return this.http.post<WaiterResponse>(`${this.baseUrl}/${userId}/role`, req);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}

