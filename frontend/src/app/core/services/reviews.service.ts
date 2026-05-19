import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ReviewResponse, CreateReviewRequest, ReviewModerationRequest, ReviewModerationResult } from '../models/review.models';

@Injectable({ providedIn: 'root' })
export class ReviewsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/restaurants`;

  getAll(restaurantId?: string): Observable<ReviewResponse[]> {
    if (restaurantId) {
      return this.http.get<ReviewResponse[]>(`${this.baseUrl}/${restaurantId}/reviews`);
    }

    return this.http.get<ReviewResponse[]>(`${environment.apiBaseUrl}/reviews`);
  }

  getById(id: string): Observable<ReviewResponse> {
    return this.http.get<ReviewResponse>(`${this.baseUrl}/${id}`);
  }

  moderate(restaurantId: string, reservationId: string, req: ReviewModerationRequest): Observable<ReviewModerationResult> {
    return this.http.post<ReviewModerationResult>(`${environment.apiBaseUrl}/restaurants/${restaurantId}/reservations/${reservationId}/review/moderate`, req).pipe(
      catchError(() => this.http.post<ReviewModerationResult>(`${environment.apiBaseUrl}/restaurants/reservations/${reservationId}/review/moderate`, req)),
    );
  }

  submit(restaurantId: string, reservationId: string, req: CreateReviewRequest): Observable<ReviewResponse> {
    return this.http.post<ReviewResponse>(`${environment.apiBaseUrl}/restaurants/${restaurantId}/reservations/${reservationId}/review`, req).pipe(
      catchError(() => this.http.post<ReviewResponse>(`${environment.apiBaseUrl}/restaurants/reservations/${reservationId}/review`, req)),
      catchError(() => this.http.post<ReviewResponse>(`${environment.apiBaseUrl}/reservations/${reservationId}/reviews`, req)),
    );
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}

