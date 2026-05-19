import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, RegisterRequest, AuthResponse, JwtPayload } from '../models/auth.models';

const TOKEN_KEY = 'access_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly baseUrl = `${environment.apiBaseUrl}/authentication`;

  private readonly _payload = signal<JwtPayload | null>(this.loadPayload());

  readonly currentUser = this._payload.asReadonly();
  readonly isLoggedIn = computed(() => {
    const p = this._payload();
    if (!p) return false;
    return p.exp * 1000 > Date.now();
  });

  hasRole(role: string): boolean {
    const groups = this._payload()?.['cognito:groups'] ?? [];
    return groups.includes(role);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  login(req: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, req).pipe(
      tap(res => {
        if (res.accessToken) {
          localStorage.setItem(TOKEN_KEY, res.accessToken);
          this._payload.set(this.decodeToken(res.accessToken));
        }
      })
    );
  }

  register(req: RegisterRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/register`, req);
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    this._payload.set(null);
    this.router.navigate(['/login']);
  }

  private loadPayload(): JwtPayload | null {
    const token = localStorage.getItem(TOKEN_KEY);
    return token ? this.decodeToken(token) : null;
  }

  private decodeToken(token: string): JwtPayload | null {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return null;
      const payload = atob(parts[1].replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(payload) as JwtPayload;
    } catch {
      return null;
    }
  }
}

