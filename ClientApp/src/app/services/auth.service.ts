import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface UserInfo {
  email: string;
  name?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private tokenKey = 'auth_token';
  private userSubject = new BehaviorSubject<UserInfo | null>(null);
  public user$: Observable<UserInfo | null> = this.userSubject.asObservable();
  private tokenValidated = false;

  constructor() {
    // Check if user is already logged in on initialization
    const token = this.getToken();
    if (token) {
      this.validateToken(token);
    }
  }

  login(): void {
    // Redirect to backend OAuth endpoint
    window.location.href = `${environment.apiUrl}/api/auth/login`;
  }

  handleAuthCallback(token: string): void {
    this.setToken(token);
    this.validateToken(token);
  }

  private async validateToken(token: string): Promise<void> {
    try {
      const response = await fetch(`${environment.apiUrl}/api/auth/validate-token`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ token })
      });

      if (response.ok) {
        const user = await response.json();
        this.userSubject.next(user);
        this.tokenValidated = true;
      } else {
        this.logout();
      }
    } catch (error) {
      console.error('Token validation failed:', error);
      this.logout();
    }
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    this.userSubject.next(null);
    this.tokenValidated = false;
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  private setToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
  }

  isAuthenticated(): boolean {
    return this.getToken() !== null && this.tokenValidated && this.userSubject.value !== null;
  }
}
