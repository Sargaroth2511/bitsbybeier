import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private darkModeSubject: BehaviorSubject<boolean>;
  public darkMode$: Observable<boolean>;
  private isBrowser: boolean;

  constructor(@Inject(PLATFORM_ID) platformId: Object) {
    this.isBrowser = isPlatformBrowser(platformId);
    this.darkModeSubject = new BehaviorSubject<boolean>(this.getInitialTheme());
    this.darkMode$ = this.darkModeSubject.asObservable();
    this.applyTheme(this.darkModeSubject.value);
  }

  private getInitialTheme(): boolean {
    if (!this.isBrowser) {
      return false;
    }
    
    // Check localStorage first
    try {
      const savedTheme = localStorage.getItem('darkMode');
      if (savedTheme !== null) {
        return savedTheme === 'true';
      }
    } catch (e) {
      // localStorage may not be available
    }
    
    // Check system preference
    try {
      return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
    } catch (e) {
      return false;
    }
  }

  public toggleTheme(): void {
    const newValue = !this.darkModeSubject.value;
    this.darkModeSubject.next(newValue);
    this.applyTheme(newValue);
    
    if (this.isBrowser) {
      try {
        localStorage.setItem('darkMode', String(newValue));
      } catch (e) {
        // localStorage may not be available
      }
    }
  }

  public isDarkMode(): boolean {
    return this.darkModeSubject.value;
  }

  private applyTheme(isDark: boolean): void {
    if (!this.isBrowser) {
      return;
    }
    
    if (isDark) {
      document.documentElement.setAttribute('data-theme', 'dark');
    } else {
      document.documentElement.setAttribute('data-theme', 'light');
    }
  }
}
