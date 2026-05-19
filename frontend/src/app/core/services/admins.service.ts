import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export type AdminUserRole = 'Admin' | 'Waiter' | 'Customer';

export interface AdminUserListItem {
  userId: string;
  email: string;
  firstName: string | null;
  lastName: string | null;
  role: AdminUserRole;
  restaurantId: string | null;
  restaurantAddress: string | null;
}

export interface PagedAdminUsersResponse {
  items: AdminUserListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface AppointAdminRequest {
  adminUserIdToAppoint: string;
  restaurantId?: string | null;
}

@Injectable({ providedIn: 'root' })
export class AdminsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/admins`;

  private readonly waitersBaseUrl = `${environment.apiBaseUrl}/waiters`;

  getUsers(role: AdminUserRole, page: number, pageSize: number): Observable<PagedAdminUsersResponse> {
    const params = new HttpParams()
      .set('role', role)
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedAdminUsersResponse>(`${this.baseUrl}/users`, { params });
  }

  appointAdmin(req: AppointAdminRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/appoint`, req);
  }

  assignWaiterRole(userId: string, restaurantId?: string): Observable<void> {
    return this.http.post<void>(`${this.waitersBaseUrl}/${userId}`, { restaurantId });
  }

  removeWaiterRole(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/users/${userId}/waiter-role`);
  }
}

