import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { isPlatformBrowser } from '@angular/common';

declare const google: any;

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    standalone: false
})
export class LoginComponent implements OnInit {
  loading = false;
  error = '';
  returnUrl = '';
  private isBrowser: boolean;

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    @Inject(PLATFORM_ID) platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
    
    // Redirect if already logged in
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/']);
    }
  }

  ngOnInit(): void {
    // Get return url from route parameters or default to '/'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';

    // Initialize Google Sign-In button if in browser
    if (this.isBrowser) {
      this.initGoogleSignIn();
    }
  }

  private initGoogleSignIn(): void {
    // Load Google Sign-In script
    if (typeof google === 'undefined') {
      const script = document.createElement('script');
      script.src = 'https://accounts.google.com/gsi/client';
      script.async = true;
      script.defer = true;
      script.onload = () => this.renderGoogleButton();
      document.head.appendChild(script);
    } else {
      this.renderGoogleButton();
    }
  }

  private renderGoogleButton(): void {
    if (typeof google !== 'undefined') {
      google.accounts.id.initialize({
        client_id: 'YOUR_GOOGLE_CLIENT_ID', // This will be replaced by environment variable
        callback: (response: any) => this.handleGoogleSignIn(response)
      });

      google.accounts.id.renderButton(
        document.getElementById('googleSignInButton'),
        { theme: 'outline', size: 'large', width: 300 }
      );
    }
  }

  private handleGoogleSignIn(response: any): void {
    this.loading = true;
    this.error = '';

    this.authService.authenticateWithGoogle(response.credential).subscribe(
      result => {
        if (result && result.token) {
          // Navigate to return url
          this.router.navigate([this.returnUrl]);
        } else {
          this.error = 'Authentication failed. Please try again.';
          this.loading = false;
        }
      },
      error => {
        this.error = 'Authentication failed. Please try again.';
        this.loading = false;
      }
    );
  }
}
