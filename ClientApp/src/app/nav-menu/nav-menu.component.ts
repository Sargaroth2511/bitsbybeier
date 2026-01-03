import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { ThemeService } from '../services/theme.service';
import { AuthService } from '../services/auth.service';

/**
 * Navigation menu component with theme toggling and user authentication.
 */
@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss'],
  standalone: true,
  imports: [
    RouterModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatListModule
  ]
})
export class NavMenuComponent {
  isExpanded = false;
  
  // Signals from services
  isDarkMode = this.themeService.darkMode;
  currentUser = this.authService.currentUser;
  isAuthenticated = this.authService.isAuthenticated;
  isAdmin = this.authService.isAdmin;

  constructor(
    private themeService: ThemeService,
    private authService: AuthService,
    private router: Router
  ) {}

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
}
