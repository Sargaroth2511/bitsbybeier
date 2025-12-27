import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { AuthService } from '../services/auth.service';
import { GoogleSignInService } from '../services/google-signin.service';
import { environment } from '../../environments/environment';

/**
 * Component for handling user login via Google Sign-In.
 */
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
    private googleSignInService: GoogleSignInService,
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

  /**
   * Initializes the component and sets up Google Sign-In.
   */
  ngOnInit(): void {
    // Get return url from route parameters or default to '/'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';

    // Initialize Google Sign-In button if in browser
    if (this.isBrowser) {
      this.initializeGoogleSignIn();
    }
  }

  /**
   * Initializes Google Sign-In and renders the button.
   */
  private async initializeGoogleSignIn(): Promise<void> {
    try {
      await this.googleSignInService.initialize();
      this.googleSignInService.renderButton(
        'googleSignInButton',
        environment.googleClientId,
        (response) => this.handleGoogleSignIn(response)
      );
    } catch (error) {
      console.error('Failed to initialize Google Sign-In', error);
      this.error = 'Failed to load Google Sign-In. Please refresh the page.';
    }
  }

  /**
   * Handles the Google Sign-In response.
   * @param response - Response from Google Sign-In containing the credential.
   */
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
