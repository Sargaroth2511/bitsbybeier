import { Component, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { CmsContent } from '../../models/cms.model';

/**
 * Reusable blog post card component
 */
@Component({
  selector: 'app-blog-post-card',
  standalone: true,
  imports: [DatePipe, MatCardModule, MatButtonModule, MatIconModule, MatChipsModule],
  template: `
    <mat-card class="post-card">
      <mat-card-header>
        <mat-card-title>{{ post().title }}</mat-card-title>
        @if (post().subtitle) {
          <mat-card-subtitle>{{ post().subtitle }}</mat-card-subtitle>
        }
      </mat-card-header>
      
      <mat-card-content>
        @if (showBadge()) {
          <div class="badge-container">
            @if (post().draft) {
              <mat-chip color="accent" highlighted>DRAFT</mat-chip>
            }
            @if (!post().draft) {
              <mat-chip color="primary" highlighted>PUBLISHED</mat-chip>
            }
          </div>
        }

        <div class="post-meta">
          <span class="author">
            <mat-icon>person</mat-icon>
            {{ post().author }}
          </span>
          <span class="date">
            <mat-icon>calendar_today</mat-icon>
            {{ post().createdAt | date:'mediumDate' }}
          </span>
          @if (post().publishAt && showScheduled()) {
            <span class="scheduled">
              <mat-icon>schedule</mat-icon>
              Scheduled: {{ post().publishAt | date:'medium' }}
            </span>
          }
        </div>
        
        <div class="post-content" [class.expanded]="expanded()">
          <ng-content></ng-content>
        </div>
      </mat-card-content>
      
      <mat-card-actions>
        <ng-content select="[actions]"></ng-content>
        @if (showToggle()) {
          <button mat-button color="primary" (click)="toggleExpanded.emit()">
            {{ expanded() ? 'Show Less' : 'Read More' }}
            <mat-icon>{{ expanded() ? 'expand_less' : 'expand_more' }}</mat-icon>
          </button>
        }
      </mat-card-actions>
    </mat-card>
  `,
  styles: [`
    .post-card {
      margin-bottom: 1.5rem;
    }

    .badge-container {
      margin-bottom: 1rem;
    }

    .post-meta {
      display: flex;
      flex-wrap: wrap;
      gap: 1rem;
      margin-bottom: 1rem;
      font-size: 0.9rem;
      color: var(--text-secondary);
    }

    .post-meta span {
      display: flex;
      align-items: center;
      gap: 0.25rem;
    }

    .post-meta mat-icon {
      font-size: 1.1rem;
      width: 1.1rem;
      height: 1.1rem;
    }

    .post-content {
      max-height: 150px;
      overflow: hidden;
      transition: max-height 0.3s ease;
    }

    .post-content.expanded {
      max-height: none;
    }

    mat-card-actions {
      display: flex;
      justify-content: space-between;
      align-items: center;
      flex-wrap: wrap;
      gap: 0.5rem;
    }
  `]
})
export class BlogPostCardComponent {
  post = input.required<CmsContent>();
  expanded = input<boolean>(false);
  showBadge = input<boolean>(false);
  showScheduled = input<boolean>(false);
  showToggle = input<boolean>(true);
  
  toggleExpanded = output<void>();
}
