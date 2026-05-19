import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DishBrief, DishDetail, CreateDishRequest, UpdateDishRequest, DishAvailabilityRequest } from '../models/dish.models';

interface ImageUploadResponse {
  url: string;
}

@Injectable({ providedIn: 'root' })
export class DishesService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiBaseUrl}`;

  getAll(page?: number, pageSize?: number): Observable<DishDetail[]> {
    let params = new HttpParams();
    if (page !== undefined) params = params.set('page', page.toString());
    if (pageSize !== undefined) params = params.set('pageSize', pageSize.toString());
    return this.http.get<DishBrief[]>(`${this.apiUrl}/dishes`, { params }).pipe(
      map(dishes => dishes.map(dish => ({ ...dish, description: null }))),
    );
  }

  getByRestaurant(_restaurantId: string, page?: number, pageSize?: number): Observable<DishDetail[]> {
    return this.getAll(page, pageSize);
  }

  getById(id: string): Observable<DishDetail> {
    return this.http.get<DishDetail>(`${this.apiUrl}/dishes/${id}`);
  }

  create(req: CreateDishRequest): Observable<DishDetail> {
    return this.http.post<DishDetail>(`${this.apiUrl}/dishes`, req);
  }

  update(id: string, req: UpdateDishRequest): Observable<DishDetail> {
    return this.http.put<DishDetail>(`${this.apiUrl}/dishes/${id}`, req);
  }

  uploadImage(id: string, file: File): Observable<ImageUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.patch<ImageUploadResponse>(`${this.apiUrl}/dishes/${id}/image`, formData);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/dishes/${id}`);
  }

  setAvailability(restaurantId: string, dishId: string, req: DishAvailabilityRequest): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/restaurants/${restaurantId}/dishes/${dishId}/availability`, req);
  }
}

