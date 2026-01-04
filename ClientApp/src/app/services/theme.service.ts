import { Injectable, Inject, PLATFORM_ID, signal, computed, Signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

/**
 * Service for managing application theme (light/dark mode).
 * Persists theme preference in local storage and applies it to the document.
 */
@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private isBrowser: boolean;
  private _darkMode = signal<boolean>(false);
  
  public readonly darkMode: Signal<boolean> = this._darkMode.asReadonly();

  constructor(@Inject(PLATFORM_ID) platformId: Object) {
    this.isBrowser = isPlatformBrowser(platformId);
    const initialTheme = this.getInitialTheme();
    this._darkMode.set(initialTheme);
    this.applyTheme(initialTheme);
  }

  /**
   * Gets the initial theme preference from localStorage or system settings.
   * @returns True if dark mode should be enabled, false otherwise.
   */
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

  /**
   * Toggles between light and dark theme.
   */
  public toggleTheme(): void {
    const newValue = !this._darkMode();
    this._darkMode.set(newValue);
    this.applyTheme(newValue);
    
    if (this.isBrowser) {
      try {
        localStorage.setItem('darkMode', String(newValue));
      } catch (e) {
        // localStorage may not be available
      }
    }
  }

  /**
   * Gets the current theme state.
   * @returns True if dark mode is active, false otherwise.
   */
  public isDarkMode(): boolean {
    return this._darkMode();
  }

  /**
   * Applies the theme by setting the data-theme attribute on the document element.
   * @param isDark - Whether to apply dark theme.
   */
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
