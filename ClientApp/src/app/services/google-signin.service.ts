import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

declare const google: any;

/**
 * Service for managing Google Sign-In integration.
 * Handles script loading and button rendering.
 */
@Injectable({
  providedIn: 'root'
})
export class GoogleSignInService {
  private isBrowser: boolean;
  private scriptLoaded = false;

  constructor(@Inject(PLATFORM_ID) platformId: Object) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  /**
   * Initializes Google Sign-In by loading the script if needed.
   * @returns Promise that resolves when Google Sign-In is ready.
   */
  async initialize(): Promise<void> {
    if (!this.isBrowser) {
      return Promise.resolve();
    }

    if (this.isGoogleLoaded()) {
      this.scriptLoaded = true;
      return Promise.resolve();
    }

    return this.loadGoogleScript();
  }

  /**
   * Renders the Google Sign-In button in the specified element.
   * @param elementId - ID of the HTML element to render the button in.
   * @param clientId - Google OAuth client ID.
   * @param callback - Callback function to handle the sign-in response.
   */
  renderButton(
    elementId: string,
    clientId: string,
    callback: (response: any) => void
  ): void {
    if (!this.isBrowser || !this.isGoogleLoaded()) {
      console.error('Google Sign-In not loaded');
      return;
    }

    google.accounts.id.initialize({
      client_id: clientId,
      callback: callback
    });

    const buttonElement = document.getElementById(elementId);
    if (buttonElement) {
      google.accounts.id.renderButton(buttonElement, {
        theme: 'outline',
        size: 'large',
        width: 300
      });
    }
  }

  /**
   * Checks if the Google Sign-In library is loaded.
   * @returns True if Google Sign-In is available, false otherwise.
   */
  private isGoogleLoaded(): boolean {
    return typeof google !== 'undefined' && !!google.accounts;
  }

  /**
   * Loads the Google Sign-In script dynamically.
   * @returns Promise that resolves when the script is loaded.
   */
  private loadGoogleScript(): Promise<void> {
    return new Promise((resolve, reject) => {
      const script = document.createElement('script');
      script.src = 'https://accounts.google.com/gsi/client';
      script.async = true;
      script.defer = true;
      script.onload = () => {
        this.scriptLoaded = true;
        resolve();
      };
      script.onerror = () => {
        reject(new Error('Failed to load Google Sign-In script'));
      };
      document.head.appendChild(script);
    });
  }
}
