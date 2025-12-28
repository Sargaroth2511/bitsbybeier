import { Component, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ThemeService } from '../services/theme.service';
import { AuthService } from '../services/auth.service';
import { User } from '../models/auth.model';

/**
 * Navigation menu component with theme toggling and user authentication.
 */
@Component({
    selector: 'app-nav-menu',
    templateUrl: './nav-menu.component.html',
    styleUrls: ['./nav-menu.component.scss'],
    standalone: false
})
export class NavMenuComponent implements OnDestroy {
  isExpanded = false;
  isDarkMode = false;
  currentUser: User | null = null;
  private themeSubscription: Subscription;
  private userSubscription: Subscription;

  constructor(
    private themeService: ThemeService,
    private authService: AuthService,
    private router: Router
  ) {
    this.themeSubscription = this.themeService.darkMode$.subscribe(isDark => {
      this.isDarkMode = isDark;
    });

    this.userSubscription = this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
  }

  /**
   * Cleans up subscriptions when component is destroyed.
   */
  ngOnDestroy() {
    if (this.themeSubscription) {
      this.themeSubscription.unsubscribe();
    }
    if (this.userSubscription) {
      this.userSubscription.unsubscribe();
    }
  }

  /**
   * Collapses the navigation menu.
   */
  collapse() {
    this.isExpanded = false;
  }

  /**
   * Toggles the navigation menu expanded state.
   */
  toggle() {
    this.isExpanded = !this.isExpanded;
  }

  /**
   * Toggles between light and dark theme.
   */
  toggleTheme() {
    this.themeService.toggleTheme();
  }

  /**
   * Logs out the current user and redirects to home page.
   */
  logout() {
    this.authService.logout();
    this.router.navigate(['/']);
  }

  /**
   * Gets whether a user is currently authenticated.
   */
  get isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  /**
   * Gets whether the current user has Admin role.
   */
  get isAdmin(): boolean {
    return this.authService.getUserRole() === 'Admin';
  }
}
