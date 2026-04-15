import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.hasValidToken()) {
    return true;
  }

  // Clear any stale token/user data so the signal + storage stay consistent.
  if (authService.getToken()) {
    authService.logout(false);
  }

  return router.createUrlTree(['/auth/login'], {
    queryParams: state.url && state.url !== '/' ? { returnUrl: state.url } : undefined,
  });
};
