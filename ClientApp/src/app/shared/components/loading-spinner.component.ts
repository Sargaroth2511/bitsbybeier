import { Component, input } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

/**
 * Reusable loading spinner component with optional message
 */
@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [MatProgressSpinnerModule],
  template: `
    <div class="loading-container">
      <mat-spinner [diameter]="diameter()"></mat-spinner>
      @if (message()) {
        <p class="loading-message">{{ message() }}</p>
      }
    </div>
  `,
  styles: [`
    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 2rem;
      gap: 1rem;
    }

    .loading-message {
      color: var(--text-secondary);
      margin: 0;
      font-size: 0.9rem;
    }
  `]
})
export class LoadingSpinnerComponent {
  message = input<string>('');
  diameter = input<number>(50);
}
