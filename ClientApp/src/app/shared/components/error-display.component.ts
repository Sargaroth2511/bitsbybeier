import { Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

/**
 * Reusable error display component
 */
@Component({
  selector: 'app-error-display',
  standalone: true,
  imports: [MatIconModule],
  template: `
    <div class="error-container" role="alert">
      <mat-icon>error_outline</mat-icon>
      <div class="error-content">
        <h3>{{ title() }}</h3>
        <p>{{ message() }}</p>
      </div>
    </div>
  `,
  styles: [`
    .error-container {
      display: flex;
      align-items: center;
      gap: 1rem;
      padding: 1rem;
      margin: 1rem 0;
      background-color: var(--error-bg, #fee);
      border: 1px solid var(--error-color, #c33);
      border-radius: 0.5rem;
      color: var(--error-text, #c33);
    }

    .error-container mat-icon {
      font-size: 2rem;
      width: 2rem;
      height: 2rem;
    }

    .error-content h3 {
      margin: 0 0 0.25rem 0;
      font-size: 1rem;
      font-weight: 600;
    }

    .error-content p {
      margin: 0;
      font-size: 0.9rem;
    }
  `]
})
export class ErrorDisplayComponent {
  title = input<string>('Error');
  message = input.required<string>();
}
