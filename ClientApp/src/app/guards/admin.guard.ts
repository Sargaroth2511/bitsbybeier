import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Guard that checks if user has Admin role before allowing route access.
 * Redirects to login if not authenticated, or home if not an admin.
 */
@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  /**
   * Determines if a route can be activated based on user's admin status.
   * @param route - Snapshot of the activated route.
   * @param state - Snapshot of the router state.
   * @returns True if user is authenticated and has Admin role, false otherwise.
   */
  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    if (!this.authService.isAuthenticated()) {
      // Not logged in, redirect to login
      this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
      return false;
    }

    const userRole = this.authService.getUserRole();
    
    if (userRole === 'Admin') {
      return true;
    }

    // Logged in but not admin, redirect to home
    this.router.navigate(['/']);
    return false;
  }
}
