import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export function roleGuard(...roles: string[]): CanActivateFn {
  return () => {
    const auth = inject(AuthService);
    if (!auth.isLoggedIn()) return inject(Router).createUrlTree(['/login']);
    const ok = roles.some(r => auth.hasRole(r));
    return ok ? true : inject(Router).createUrlTree(['/forbidden']);
  };
}

