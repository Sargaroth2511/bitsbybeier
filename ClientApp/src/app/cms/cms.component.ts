import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../services/auth.service';
import { CmsService } from '../services/cms.service';
import { User } from '../models/auth.model';
import { CmsContent } from '../models/cms.model';

/**
 * Component for the Content Management System (CMS) interface.
 * Accessible only to users with Admin role.
 */
@Component({
    selector: 'app-cms',
    templateUrl: './cms.component.html',
    styleUrls: ['./cms.component.scss'],
    standalone: false
})
export class CmsComponent implements OnInit {
  currentUser: User | null = null;
  content: CmsContent[] = [];
  loading = false;
  error = '';
  creating = false;
  showPreview = false;
  today = new Date();
  
  contentForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private cmsService: CmsService,
    private snackBar: MatSnackBar,
    private sanitizer: DomSanitizer
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
   * Initializes the component by subscribing to current user and loading content.
   */
  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
      if (user && user.name) {
        this.contentForm.patchValue({ author: user.name });
      }
    });
    
    this.loadContent();
  }

  /**
   * Loads CMS content from the API.
   */
  loadContent(): void {
    this.loading = true;
    this.error = '';
    
    this.cmsService.getAllContent().subscribe({
      next: (data) => {
        this.content = data;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load content';
        this.loading = false;
        console.error('Error loading content', error);
      }
    });
  }

  /**
   * Creates new content.
   */
  createContent(): void {
    if (this.contentForm.invalid) {
      return;
    }

    this.creating = true;
    const formValue = this.contentForm.value;
    
    this.cmsService.createContent(formValue).subscribe({
      next: (response) => {
        this.snackBar.open(
          formValue.draft ? 'Draft saved successfully' : 'Content published successfully',
          'Close',
          { duration: 3000 }
        );
        this.resetForm();
        this.loadContent();
        this.creating = false;
      },
      error: (error) => {
        console.error('Error creating content', error);
        this.snackBar.open('Failed to create content', 'Close', { duration: 3000 });
        this.creating = false;
      }
    });
  }

  /**
   * Resets the form.
   */
  resetForm(): void {
    this.contentForm.reset({
      author: this.currentUser?.name || '',
      draft: true
    });
    this.showPreview = false;
  }

  /**
   * Edits existing content by loading it into the form.
   */
  editContent(content: CmsContent): void {
    this.contentForm.patchValue({
      author: content.author,
      title: content.title,
      subtitle: content.subtitle || '',
      content: content.content,
      draft: content.draft
    });
    this.showPreview = false;
    // Scroll to form
    window.scrollTo({ top: 0, behavior: 'smooth' });
    this.snackBar.open('Content loaded for editing. Modify and save to update.', 'Close', { duration: 3000 });
  }

  /**
   * Shows content preview.
   */
  previewContent(): void {
    this.showPreview = true;
  }

  /**
   * Gets formatted preview HTML.
   */
  getFormattedPreview(): SafeHtml {
    const content = this.contentForm.get('content')?.value || '';
    let html = content
      .replace(/^### (.*$)/gim, '<h3>$1</h3>')
      .replace(/^## (.*$)/gim, '<h2>$1</h2>')
      .replace(/^# (.*$)/gim, '<h1>$1</h1>')
      .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
      .replace(/\*(.*?)\*/g, '<em>$1</em>')
      .replace(/\[([^\]]+)\]\(([^\)]+)\)/g, '<a href="$2" target="_blank">$1</a>')
      .replace(/\n/g, '<br>');
    
    return this.sanitizer.sanitize(1, html) || '';
  }
}
