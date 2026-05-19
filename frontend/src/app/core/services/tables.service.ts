import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TableBrief, AddTableRequest } from '../models/table.models';

@Injectable({ providedIn: 'root' })
export class TablesService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiBaseUrl}`;

  getByRestaurant(restaurantId: string): Observable<TableBrief[]> {
    return this.http.get<TableBrief[]>(`${this.apiUrl}/restaurants/${restaurantId}/tables`);
  }

  create(restaurantId: string, req: AddTableRequest): Observable<TableBrief> {
    return this.http.post<TableBrief>(`${this.apiUrl}/restaurants/${restaurantId}/tables`, req);
  }

  delete(restaurantId: string, tableNumber: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/restaurants/${restaurantId}/tables/${tableNumber}`);
  }
}

