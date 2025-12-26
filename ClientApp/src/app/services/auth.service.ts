import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { isPlatformBrowser } from '@angular/common';

declare const google: any;

export interface User {
  email: string;
  name: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject: BehaviorSubject<User | null>;
  public currentUser$: Observable<User | null>;
  private isBrowser: boolean;

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
    const user = this.isBrowser ? this.getUserFromToken() : null;
    this.currentUserSubject = new BehaviorSubject<User | null>(user);
    this.currentUser$ = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  public get token(): string | null {
    if (!this.isBrowser) {
      return null;
    }
    return localStorage.getItem('jwt_token');
  }

  public isAuthenticated(): boolean {
    return !!this.token;
  }

  public authenticateWithGoogle(idToken: string): Observable<any> {
    return this.http.post<any>('/api/auth/google', { idToken }).pipe(
      tap(response => {
        if (response.token) {
          this.setToken(response.token);
          this.currentUserSubject.next({
            email: response.email,
            name: response.name
          });
        }
      }),
      catchError(error => {
        console.error('Authentication failed', error);
        return of(null);
      })
    );
  }

  public logout(): void {
    if (this.isBrowser) {
      localStorage.removeItem('jwt_token');
    }
    this.currentUserSubject.next(null);
  }

  public getUser(): Observable<User> {
    return this.http.get<User>('/api/auth/user');
  }

  private setToken(token: string): void {
    if (this.isBrowser) {
      localStorage.setItem('jwt_token', token);
    }
  }

  private getUserFromToken(): User | null {
    const token = this.token;
    if (!token) {
      return null;
    }

    try {
      // Decode JWT token
      const payload = JSON.parse(atob(token.split('.')[1]));
      return {
        email: payload.email || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
        name: payload.name || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name']
      };
    } catch (e) {
      return null;
    }
  }
}
