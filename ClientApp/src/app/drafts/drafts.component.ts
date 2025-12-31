import { Component, OnInit } from '@angular/core';
import { CmsService } from '../services/cms.service';
import { CmsContent } from '../models/cms.model';
import { MatSnackBar } from '@angular/material/snack-bar';

/**
 * Component for managing draft content (Admin only).
 */
@Component({
    selector: 'app-drafts',
    templateUrl: './drafts.component.html',
    styleUrls: ['./drafts.component.scss'],
    standalone: false
})
export class DraftsComponent implements OnInit {
  drafts: CmsContent[] = [];
  loading = false;
  error = '';

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
    this.loading = true;
    this.error = '';
    
    this.cmsService.getDraftContent().subscribe({
      next: (data) => {
        this.drafts = data;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load draft content';
        this.loading = false;
        console.error('Error loading drafts', error);
        this.snackBar.open('Failed to load drafts', 'Close', { duration: 3000 });
      }
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
   */
  schedulePublish(draft: CmsContent): void {
    const dateStr = prompt('Enter publication date and time (YYYY-MM-DD HH:MM):');
    if (!dateStr) return;

    try {
      const publishAt = new Date(dateStr).toISOString();
      this.cmsService.updateContent(draft.id, { publishAt }).subscribe({
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

  /**
   * Formats markdown content for preview (basic implementation).
   */
  getPreviewText(content: string): string {
    // Simple markdown removal for preview
    return content
      .replace(/#+\s/g, '') // Remove headers
      .replace(/\*\*/g, '') // Remove bold
      .replace(/\*/g, '') // Remove italic
      .replace(/\[([^\]]+)\]\([^\)]+\)/g, '$1') // Remove links
      .substring(0, 200); // Truncate
  }
}
