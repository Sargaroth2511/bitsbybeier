import { Component, OnInit, signal, computed } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { AuthService } from '../services/auth.service';
import { CmsService } from '../services/cms.service';
import { CmsContent } from '../models/cms.model';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner.component';
import { ErrorDisplayComponent } from '../shared/components/error-display.component';

/**
 * Component for the Content Management System (CMS) interface.
 * Accessible only to users with Admin role.
 */
@Component({
  selector: 'app-cms',
  templateUrl: './cms.component.html',
  styleUrls: ['./cms.component.scss'],
  standalone: true,
  imports: [
    DatePipe,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatSnackBarModule,
    LoadingSpinnerComponent,
    ErrorDisplayComponent
  ]
})
export class CmsComponent implements OnInit {
  currentUser = this.authService.currentUser;
  content = signal<CmsContent[]>([]);
  loading = signal(false);
  error = signal('');
  creating = signal(false);
  showPreview = signal(false);
  today = new Date();
  editingContentId = signal<number | null>(null);
  
  contentForm: FormGroup;
  
  previewContent = computed(() => {
    const content = this.contentForm.get('content')?.value || '';
    return this.formatMarkdown(content);
  });

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private cmsService: CmsService,
    private snackBar: MatSnackBar
  ) {
    this.contentForm = this.fb.group({
      author: ['', Validators.required],
      title: ['', Validators.required],
      subtitle: [''],
      content: ['', Validators.required],
      draft: [true]
    });
  }

  /**
   * Initializes the component by setting up user and loading content.
   */
  ngOnInit(): void {
    const user = this.currentUser();
    if (user && user.name) {
      this.contentForm.patchValue({ author: user.name });
    }
    
    this.loadContent();
  }

  /**
   * Loads CMS content from the API.
   */
  loadContent(): void {
    this.loading.set(true);
    this.error.set('');
    
    this.cmsService.getAllContent().subscribe({
      next: (data) => {
        this.content.set(data);
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set('Failed to load content');
        this.loading.set(false);
        console.error('Error loading content', error);
      }
    });
  }

  /**
   * Creates new content or updates existing content.
   */
  createContent(): void {
    if (this.contentForm.invalid) {
      return;
    }

    this.creating.set(true);
    const formValue = this.contentForm.value;
    const editingId = this.editingContentId();
    
    if (editingId) {
      // Update existing content with full fields
      this.cmsService.updateContentFull(editingId, {
        author: formValue.author,
        title: formValue.title,
        subtitle: formValue.subtitle,
        content: formValue.content,
        draft: formValue.draft,
        active: true
      }).subscribe({
        next: () => {
          this.snackBar.open(
            formValue.draft ? 'Draft updated successfully' : 'Content updated and published',
            'Close',
            { duration: 3000 }
          );
          this.resetForm();
          this.loadContent();
          this.creating.set(false);
        },
        error: (error) => {
          console.error('Error updating content', error);
          this.snackBar.open('Failed to update content', 'Close', { duration: 3000 });
          this.creating.set(false);
        }
      });
    } else {
      // Create new content
      this.cmsService.createContent(formValue).subscribe({
        next: (response) => {
          this.snackBar.open(
            formValue.draft ? 'Draft saved successfully' : 'Content published successfully',
            'Close',
            { duration: 3000 }
          );
          this.resetForm();
          this.loadContent();
          this.creating.set(false);
        },
        error: (error) => {
          console.error('Error creating content', error);
          this.snackBar.open('Failed to create content', 'Close', { duration: 3000 });
          this.creating.set(false);
        }
      });
    }
  }

  /**
   * Resets the form.
   */
  resetForm(): void {
    const user = this.currentUser();
    this.contentForm.reset({
      author: user?.name || '',
      draft: true
    });
    this.showPreview.set(false);
    this.editingContentId.set(null);
  }

  /**
   * Edits existing content by loading it into the form.
   */
  editContent(content: CmsContent): void {
    this.editingContentId.set(content.id);
    this.contentForm.patchValue({
      author: content.author,
      title: content.title,
      subtitle: content.subtitle || '',
      content: content.content,
      draft: content.draft
    });
    this.showPreview.set(false);
    // Scroll to form
    window.scrollTo({ top: 0, behavior: 'smooth' });
    this.snackBar.open('Content loaded for editing. Modify and save to update.', 'Close', { duration: 3000 });
  }

  /**
   * Shows content preview.
   */
  togglePreview(): void {
    this.showPreview.update(val => !val);
  }

  /**
   * Formats markdown content to HTML.
   */
  private formatMarkdown(content: string): string {
    let html = content
      .replace(/^### (.*$)/gim, '<h3>$1</h3>')
      .replace(/^## (.*$)/gim, '<h2>$1</h2>')
      .replace(/^# (.*$)/gim, '<h1>$1</h1>')
      .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
      .replace(/\*(.*?)\*/g, '<em>$1</em>')
      .replace(/\[([^\]]+)\]\(([^\)]+)\)/g, '<a href="$2" target="_blank">$1</a>')
      .replace(/\n/g, '<br>');
    
    return html;
  }
}
