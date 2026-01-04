import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Functional guard that checks if user has Admin role before allowing route access.
 * Redirects to home page if user is not an admin.
 */
export const adminGuard: CanActivateFn = () => {
  const router = inject(Router);
  const authService = inject(AuthService);

  if (authService.isAdmin()) {
    return true;
  }

  // Not an admin so redirect to home page
  router.navigate(['/']);
  return false;
};
