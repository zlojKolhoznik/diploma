import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RestaurantBrief, RestaurantDetail, CreateRestaurantRequest, UpdateRestaurantRequest } from '../models/restaurant.models';

interface ImageUploadResponse {
  url: string;
}

@Injectable({ providedIn: 'root' })
export class RestaurantsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/restaurants`;

  getAll(city?: string, page?: number, pageSize?: number): Observable<RestaurantBrief[]> {
    let params = new HttpParams();
    if (city) params = params.set('city', city);
    if (page !== undefined) params = params.set('page', page.toString());
    if (pageSize !== undefined) params = params.set('pageSize', pageSize.toString());
    return this.http.get<RestaurantBrief[]>(this.baseUrl, { params });
  }

  getById(id: string): Observable<RestaurantDetail> {
    return this.http.get<RestaurantDetail>(`${this.baseUrl}/${id}`);
  }

  create(req: CreateRestaurantRequest): Observable<RestaurantDetail> {
    return this.http.post<RestaurantDetail>(this.baseUrl, req);
  }

  update(id: string, req: UpdateRestaurantRequest): Observable<RestaurantDetail> {
    return this.http.put<RestaurantDetail>(`${this.baseUrl}/${id}`, req);
  }

  uploadImage(id: string, file: File): Observable<ImageUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.patch<ImageUploadResponse>(`${this.baseUrl}/${id}/image`, formData);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}

