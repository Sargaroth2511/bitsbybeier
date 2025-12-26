import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-auth-callback',
  template: `
    <div class="container mt-5">
      <div class="text-center">
        <h2>Completing authentication...</h2>
        <div class="spinner-border" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
      </div>
    </div>
  `,
  standalone: false
})
export class AuthCallbackComponent implements OnInit {
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const token = params['token'];
      
      if (token) {
        this.authService.handleAuthCallback(token);
        // Redirect to CMS page after successful authentication
        this.router.navigate(['/cms']);
      } else {
        // No token found, redirect to home
        this.router.navigate(['/']);
      }
    });
  }
}
