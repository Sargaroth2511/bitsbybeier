import { Component, OnInit } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
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
  drafts: (CmsContent & { expanded?: boolean })[] = [];
  loading = false;
  error = '';

  constructor(
    private cmsService: CmsService,
    private snackBar: MatSnackBar,
    private sanitizer: DomSanitizer
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
        this.drafts = data.map(draft => ({ ...draft, expanded: false }));
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
   * Toggles the expanded state of a draft.
   */
  toggleExpanded(draft: CmsContent & { expanded?: boolean }): void {
    draft.expanded = !draft.expanded;
  }

  /**
   * Formats content with basic markdown-to-HTML conversion.
   */
  formatContent(content: string): SafeHtml {
    // Basic markdown to HTML conversion
    let html = content
      // Headers
      .replace(/^### (.*$)/gim, '<h3>$1</h3>')
      .replace(/^## (.*$)/gim, '<h2>$1</h2>')
      .replace(/^# (.*$)/gim, '<h1>$1</h1>')
      // Bold
      .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
      // Italic
      .replace(/\*(.*?)\*/g, '<em>$1</em>')
      // Links
      .replace(/\[([^\]]+)\]\(([^\)]+)\)/g, '<a href="$2" target="_blank">$1</a>')
      // Line breaks
      .replace(/\n/g, '<br>');
    
    return this.sanitizer.sanitize(1, html) || '';
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
