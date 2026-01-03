import { Injectable, Inject, PLATFORM_ID, signal, computed, Signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { isPlatformBrowser } from '@angular/common';
import { User, UserRole, AuthenticationResponse, GoogleTokenRequest } from '../models/auth.model';
import { API_ENDPOINTS, STORAGE_KEYS } from '../constants/api.constants';

/**
 * Service for managing user authentication and session state.
 */
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private isBrowser: boolean;
  private _currentUser = signal<User | null>(null);
  
  public readonly currentUser: Signal<User | null> = this._currentUser.asReadonly();
  
  // isAuthenticated depends on currentUser signal AND checks token validity
  public readonly isAuthenticated = computed(() => {
    const user = this._currentUser();
    if (!user) return false;
    // Also verify token still exists and is valid
    return !!this.token;
  });
  
  // userRole computed from currentUser signal
  public readonly userRole = computed(() => {
    const user = this._currentUser();
    if (!user || !user.role) return null;
    return user.role;
  });
  
  // isAdmin checks if userRole matches Admin enum value
  public readonly isAdmin = computed(() => this.userRole() === UserRole.Admin);

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
    const user = this.isBrowser ? this.getUserFromToken() : null;
    this._currentUser.set(user);
  }

  /**
   * Gets the current user value synchronously.
   */
  public get currentUserValue(): User | null {
    return this._currentUser();
  }

  /**
   * Gets the JWT token from storage if valid.
   * @returns JWT token string or null if not available or expired.
   */
  public get token(): string | null {
    if (!this.isBrowser) {
      return null;
    }
    const token = localStorage.getItem(STORAGE_KEYS.JWT_TOKEN);
    
    // Validate token hasn't expired
    if (token && this.isTokenExpired(token)) {
      this.logout();
      return null;
    }
    
    return token;
  }

  /**
   * Checks if the user is authenticated.
   * @returns True if user has a valid token, false otherwise.
   */
  public isAuthenticatedValue(): boolean {
    return !!this.token;
  }

  /**
   * Authenticates a user using Google OAuth ID token.
   * @param idToken - Google ID token from client-side authentication.
   * @returns Observable with authentication response or null on error.
   */
  public authenticateWithGoogle(idToken: string): Observable<AuthenticationResponse | null> {
    const request: GoogleTokenRequest = { idToken };
    
    return this.http.post<AuthenticationResponse>(API_ENDPOINTS.AUTH.GOOGLE_LOGIN, request).pipe(
      tap(response => {
        if (response.token) {
          this.setToken(response.token);
          this._currentUser.set({
            email: response.email,
            name: response.name,
            role: response.role,
            userId: response.userId
          });
        }
      }),
      catchError(error => {
        console.error('Authentication failed', error);
        return of(null);
      })
    );
  }

  /**
   * Gets the current user's role from the JWT token.
   * @returns User role string or null if not available.
   */
  public getUserRole(): string | null {
    if (!this.isBrowser) {
      return null;
    }
    
    const token = localStorage.getItem(STORAGE_KEYS.JWT_TOKEN);
    if (!token) {
      return null;
    }

    try {
      const payload = this.parseTokenPayload(token);
      return payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.role || null;
    } catch (e) {
      return null;
    }
  }

  /**
   * Logs out the current user by removing their token and clearing state.
   */
  public logout(): void {
    if (this.isBrowser) {
      localStorage.removeItem(STORAGE_KEYS.JWT_TOKEN);
    }
    this._currentUser.set(null);
  }

  /**
   * Gets the current user's information from the API.
   * @returns Observable with user data.
   */
  public getUser(): Observable<User> {
    return this.http.get<User>(API_ENDPOINTS.AUTH.GET_USER);
  }

  /**
   * Stores the JWT token in local storage.
   * @param token - JWT token to store.
   */
  private setToken(token: string): void {
    if (this.isBrowser) {
      localStorage.setItem(STORAGE_KEYS.JWT_TOKEN, token);
    }
  }

  /**
   * Extracts user information from the JWT token payload.
   * Note: This is for UI display purposes only. The token payload is decoded
   * without signature verification - the authoritative security check happens
   * on the server side when the token is sent with API requests.
   * @returns User object or null if token is invalid.
   */
  private getUserFromToken(): User | null {
    if (!this.isBrowser) {
      return null;
    }
    
    const token = localStorage.getItem(STORAGE_KEYS.JWT_TOKEN);
    if (!token) {
      return null;
    }

    try {
      const payload = this.parseTokenPayload(token);
      return {
        email: payload.email || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
        name: payload.name || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
        role: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.role
      };
    } catch (e) {
      return null;
    }
  }

  /**
   * Checks if a JWT token has expired.
   * @param token - JWT token to check.
   * @returns True if expired, false otherwise.
   */
  private isTokenExpired(token: string): boolean {
    try {
      const payload = this.parseTokenPayload(token);
      const exp = payload.exp;
      if (!exp) {
        return false;
      }
      // Check if token has expired (exp is in seconds, Date.now() is in milliseconds)
      return Date.now() >= exp * 1000;
    } catch (e) {
      return true; // If we can't parse the token, consider it expired
    }
  }

  /**
   * Parses the JWT token payload.
   * @param token - JWT token to parse.
   * @returns Decoded token payload.
   */
  private parseTokenPayload(token: string): any {
    return JSON.parse(atob(token.split('.')[1]));
  }
}
