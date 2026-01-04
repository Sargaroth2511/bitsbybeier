import { Component, OnInit, Inject, PLATFORM_ID, signal } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { AuthService } from '../services/auth.service';
import { GoogleSignInService } from '../services/google-signin.service';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner.component';
import { ErrorDisplayComponent } from '../shared/components/error-display.component';
import { environment } from '../../environments/environment';

/**
 * Component for handling user login via Google Sign-In.
 */
@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  standalone: true,
  imports: [LoadingSpinnerComponent, ErrorDisplayComponent]
})
export class LoginComponent implements OnInit {
  loading = signal(false);
  error = signal('');
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
    if (this.authService.isAuthenticatedValue()) {
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
      this.error.set('Failed to load Google Sign-In. Please refresh the page.');
    }
  }

  /**
   * Handles the Google Sign-In response.
   * @param response - Response from Google Sign-In containing the credential.
   */
  private handleGoogleSignIn(response: any): void {
    this.loading.set(true);
    this.error.set('');

    this.authService.authenticateWithGoogle(response.credential).subscribe(
      result => {
        if (result && result.token) {
          // Navigate to return url
          this.router.navigate([this.returnUrl]);
        } else {
          this.error.set('Authentication failed. Please try again.');
          this.loading.set(false);
        }
      },
      error => {
        this.error.set('Authentication failed. Please try again.');
        this.loading.set(false);
      }
    );
  }
}
