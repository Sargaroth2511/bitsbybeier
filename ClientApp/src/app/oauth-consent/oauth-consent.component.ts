import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient, HttpParams } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-oauth-consent',
  templateUrl: './oauth-consent.component.html',
  styleUrls: ['./oauth-consent.component.scss'],
  standalone: true,
  imports: [CommonModule]
})
export class OAuthConsentComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private http = inject(HttpClient);

  loading = true;
  error = '';

  ngOnInit() {
    // Get all OAuth parameters from query string
    const params = this.route.snapshot.queryParams;
    
    // Call the OAuth authorize endpoint with JWT token (via HTTP interceptor)
    let httpParams = new HttpParams();
    Object.keys(params).forEach(key => {
      httpParams = httpParams.append(key, params[key]);
    });

    this.http.get('/api/oauth/authorize', { params: httpParams, responseType: 'text' })
      .subscribe({
        next: (redirectUrl) => {
          // The backend returns the redirect URL, now redirect the browser
          window.location.href = redirectUrl;
        },
        error: (err) => {
          this.loading = false;
          this.error = err.error?.error || 'Authorization failed. Please try again.';
        }
      });
  }
}
