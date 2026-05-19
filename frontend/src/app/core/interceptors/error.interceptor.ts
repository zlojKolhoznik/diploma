import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      switch (error.status) {
        case 401:
          auth.logout();
          break;
        case 403:
          router.navigate(['/forbidden']);
          break;
        case 404:
          // Don't auto-navigate for API 404s — let components handle them
          break;
        default:
          break;
      }
      return throwError(() => error);
    })
  );
};

