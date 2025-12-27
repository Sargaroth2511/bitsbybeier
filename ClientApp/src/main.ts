import { enableProdMode, provideZoneChangeDetection } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

const providers = [
  { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] }
];

if (environment.production) {
  enableProdMode();
  
  // Suppress Google OAuth COOP warnings in production
  const originalWarn = console.warn;
  console.warn = function(...args: any[]) {
    const message = args.join(' ');
    if (message.includes('Cross-Origin-Opener-Policy') || 
        message.includes('window.postMessage')) {
      return;
    }
    originalWarn.apply(console, args);
  };
}

platformBrowserDynamic(providers).bootstrapModule(AppModule, { applicationProviders: [provideZoneChangeDetection()], })
  .catch(err => console.log(err));
