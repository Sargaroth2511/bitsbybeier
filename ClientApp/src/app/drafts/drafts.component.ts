import { Component, OnInit, signal } from '@angular/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CmsService } from '../services/cms.service';
import { CmsContent } from '../models/cms.model';
import { BlogPostCardComponent } from '../shared/components/blog-post-card.component';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner.component';
import { ErrorDisplayComponent } from '../shared/components/error-display.component';
import { MarkdownPipe } from '../shared/pipes/markdown.pipe';

/**
 * Component for managing draft content (Admin only).
 */
@Component({
  selector: 'app-drafts',
  templateUrl: './drafts.component.html',
  styleUrls: ['./drafts.component.scss'],
  standalone: true,
  imports: [
    MatIconModule,
    MatButtonModule,
    MatSnackBarModule,
    BlogPostCardComponent,
    LoadingSpinnerComponent,
    ErrorDisplayComponent,
    MarkdownPipe
  ]
})
export class DraftsComponent implements OnInit {
  drafts = signal<(CmsContent & { expanded?: boolean })[]>([]);
  loading = signal(false);
  error = signal('');

  constructor(
    private cmsService: CmsService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadDrafts();
  }

  /**
   * Loads draft content from the API.
   */
  loadDrafts(): void {
    this.loading.set(true);
    this.error.set('');
    
    this.cmsService.getDraftContent().subscribe({
      next: (data) => {
        this.drafts.set(data.map(draft => ({ ...draft, expanded: false })));
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set('Failed to load draft content');
        this.loading.set(false);
        console.error('Error loading drafts', error);
        this.snackBar.open('Failed to load drafts', 'Close', { duration: 3000 });
      }
    });
  }

  /**
   * Toggles the expanded state of a draft.
   */
  toggleExpanded(index: number): void {
    this.drafts.update(drafts => {
      const newDrafts = [...drafts];
      newDrafts[index] = { ...newDrafts[index], expanded: !newDrafts[index].expanded };
      return newDrafts;
    });
  }

  /**
   * Publishes a draft immediately.
   */
  publishNow(draft: CmsContent): void {
    this.cmsService.updateContent(draft.id, { draft: false, active: true }).subscribe({
      next: () => {
        this.snackBar.open('Content published successfully', 'Close', { duration: 3000 });
        this.loadDrafts();
      },
      error: (error) => {
        console.error('Error publishing content', error);
        this.snackBar.open('Failed to publish content', 'Close', { duration: 3000 });
      }
    });
  }

  /**
   * Schedules a draft for future publication.
   * TODO: Replace browser prompt with Angular Material DateTimePicker for better UX
   */
  schedulePublish(draft: CmsContent): void {
    const dateStr = prompt('Enter publication date and time (YYYY-MM-DD HH:MM):');
    if (!dateStr) return;

    try {
      const publishAt = new Date(dateStr);
      // Validate the date
      if (isNaN(publishAt.getTime())) {
        this.snackBar.open('Invalid date format', 'Close', { duration: 3000 });
        return;
      }
      
      this.cmsService.updateContent(draft.id, { publishAt: publishAt.toISOString() }).subscribe({
        next: () => {
          this.snackBar.open('Publication scheduled successfully', 'Close', { duration: 3000 });
          this.loadDrafts();
        },
        error: (error) => {
          console.error('Error scheduling publication', error);
          this.snackBar.open('Failed to schedule publication', 'Close', { duration: 3000 });
        }
      });
    } catch (e) {
      this.snackBar.open('Invalid date format', 'Close', { duration: 3000 });
    }
  }

  /**
   * Keeps content as draft (no action, just confirmation).
   */
  keepAsDraft(draft: CmsContent): void {
    this.snackBar.open('Content kept as draft', 'Close', { duration: 2000 });
  }

  /**
   * Deletes a draft.
   * TODO: Replace browser confirm with MatDialog for better UX consistency
   */
  deleteDraft(draft: CmsContent): void {
    if (!confirm(`Are you sure you want to delete "${draft.title}"?`)) {
      return;
    }

    this.cmsService.deleteContent(draft.id).subscribe({
      next: () => {
        this.snackBar.open('Draft deleted successfully', 'Close', { duration: 3000 });
        this.loadDrafts();
      },
      error: (error) => {
        console.error('Error deleting draft', error);
        this.snackBar.open('Failed to delete draft', 'Close', { duration: 3000 });
      }
    });
  }
}
