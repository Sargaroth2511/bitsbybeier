import { Component, OnInit, signal, computed } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
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
  uploadingImages = signal(false);
  uploadedImages = signal<Array<{id: number, fileName: string, fileSize: number}>>([]);
  contentText = signal<string>(''); // Signal for reactive content
  today = new Date();
  editingContentId = signal<number | null>(null);
  
  contentForm: FormGroup;
  
  previewContent = computed(() => {
    const content = this.contentText(); // Use signal instead of form value
    return this.formatMarkdown(content);
  });

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private cmsService: CmsService,
    private snackBar: MatSnackBar,
    private route: ActivatedRoute,
    private router: Router
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
    
    // Subscribe to content changes for live preview
    this.contentForm.get('content')?.valueChanges.subscribe(value => {
      this.contentText.set(value || '');
    });
    
    // Initialize content text
    this.contentText.set(this.contentForm.get('content')?.value || '');
    
    // Check if we have a content ID in the route
    const contentId = this.route.snapshot.paramMap.get('id');
    if (contentId) {
      this.loadContentById(parseInt(contentId, 10));
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
      // Create new content with image IDs
      const imageIds = this.uploadedImages().map(img => img.id);
      this.cmsService.createContent({
        ...formValue,
        imageIds: imageIds.length > 0 ? imageIds : undefined
      }).subscribe({
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
    this.uploadedImages.set([]);
    this.contentText.set('');
    
    // Clear URL parameter
    this.router.navigate(['/cms'], { replaceUrl: true });
  }

  /**
   * Loads content by ID from API.
   */
  private loadContentById(id: number): void {
    this.loading.set(true);
    this.cmsService.getAllContent().subscribe({
      next: (data) => {
        const content = data.find(c => c.id === id);
        if (content) {
          this.editContent(content);
        } else {
          this.snackBar.open('Content not found', 'Close', { duration: 3000 });
          this.router.navigate(['/cms']);
        }
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading content', error);
        this.snackBar.open('Failed to load content', 'Close', { duration: 3000 });
        this.router.navigate(['/cms']);
        this.loading.set(false);
      }
    });
  }

  /**
   * Edits existing content by loading it into the form.
   */
  editContent(content: CmsContent): void {
    this.editingContentId.set(content.id);
    
    // Update URL to include content ID
    this.router.navigate(['/cms', content.id], { replaceUrl: true });
    
    this.contentForm.patchValue({
      author: content.author,
      title: content.title,
      subtitle: content.subtitle || '',
      content: content.content,
      draft: content.draft
    });
    
    // Update content signal for preview
    this.contentText.set(content.content);
    
    // Parse markdown to find image references
    const imageRegex = /!\[([^\]]*)\]\(\/api\/images\/(\d+)\)/g;
    const markdownImageIds = new Set<number>();
    let match;
    
    while ((match = imageRegex.exec(content.content)) !== null) {
      markdownImageIds.add(parseInt(match[2], 10));
    }
    
    // Combine imageIds from API and images found in markdown
    const allImageIds = new Set([
      ...(content.imageIds || []),
      ...Array.from(markdownImageIds)
    ]);
    
    // Load image metadata for all images
    if (allImageIds.size > 0) {
      const imagePromises = Array.from(allImageIds).map(id => 
        this.cmsService.getImageMetadata(id).toPromise().catch(() => null)
      );
      
      Promise.all(imagePromises).then(images => {
        const validImages = images.filter(img => img !== null) as Array<{id: number, fileName: string, fileSize: number}>;
        this.uploadedImages.set(validImages);
      }).catch(error => {
        console.error('Error loading images', error);
        this.uploadedImages.set([]);
      });
    } else {
      this.uploadedImages.set([]);
    }
    
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
   * Inserts an image reference into the markdown content at cursor position.
   */
  insertImageIntoContent(imageId: number, fileName: string): void {
    const textarea = document.querySelector('textarea[formControlName="content"]') as HTMLTextAreaElement;
    const currentContent = this.contentForm.get('content')?.value || '';
    
    // Create markdown image syntax
    const imageMarkdown = `![${fileName}](/api/images/${imageId})`;
    
    if (textarea) {
      const cursorPos = textarea.selectionStart || currentContent.length;
      const newContent = 
        currentContent.substring(0, cursorPos) + 
        '\n' + imageMarkdown + '\n' + 
        currentContent.substring(cursorPos);
      
      this.contentForm.patchValue({ content: newContent });
      
      // Set cursor after inserted image
      setTimeout(() => {
        textarea.focus();
        textarea.setSelectionRange(cursorPos + imageMarkdown.length + 2, cursorPos + imageMarkdown.length + 2);
      }, 0);
    } else {
      // Fallback: append to content
      this.contentForm.patchValue({ 
        content: currentContent + '\n' + imageMarkdown + '\n' 
      });
    }
    
    this.snackBar.open('Image inserted into content', 'Close', { duration: 2000 });
  }

  /**
   * Formats markdown content to HTML.
   */
  private formatMarkdown(content: string): string {
    // Handle images first - convert to img tags
    let html = content.replace(/!\[([^\]]*)\]\(([^\)]+)\)/g, (match, alt, src) => {
      return `<img src="${src}" alt="${alt}" class="content-image" style="max-width: 100%; height: auto; margin: 1rem 0; display: block;" />`;
    });
    
    // Then handle other markdown
    html = html
      .replace(/^### (.*$)/gim, '<h3>$1</h3>')
      .replace(/^## (.*$)/gim, '<h2>$1</h2>')
      .replace(/^# (.*$)/gim, '<h1>$1</h1>')
      .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
      .replace(/\*(.*?)\*/g, '<em>$1</em>')
      .replace(/\[([^\]]+)\]\(([^\)]+)\)/g, '<a href="$2" target="_blank">$1</a>')
      .replace(/\n/g, '<br>');
    
    return html;
  }

  /**
   * Handles file selection from input.
   */
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.uploadFiles(Array.from(input.files));
      input.value = ''; // Reset input
    }
  }

  /**
   * Handles file drop event.
   */
  onFileDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    
    if (event.dataTransfer?.files) {
      this.uploadFiles(Array.from(event.dataTransfer.files));
    }
  }

  /**
   * Handles drag over event.
   */
  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
  }

  /**
   * Handles drag leave event.
   */
  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
  }

  /**
   * Uploads files to the server.
   */
  private uploadFiles(files: File[]): void {
    const imageFiles = files.filter(f => f.type.startsWith('image/'));
    
    if (imageFiles.length === 0) {
      this.snackBar.open('Please select image files only', 'Close', { duration: 3000 });
      return;
    }

    // Check file sizes
    const oversized = imageFiles.filter(f => f.size > 10 * 1024 * 1024);
    if (oversized.length > 0) {
      this.snackBar.open(`Some images exceed 10MB: ${oversized.map(f => f.name).join(', ')}`, 'Close', { duration: 5000 });
      return;
    }

    this.uploadingImages.set(true);

    // Upload each file
    let completed = 0;
    imageFiles.forEach(file => {
      const reader = new FileReader();
      reader.onload = () => {
        const base64 = (reader.result as string).split(',')[1]; // Remove data URI prefix
        
        this.cmsService.uploadImage({
          base64Data: base64,
          fileName: file.name,
          contentType: file.type
        }).subscribe({
          next: (response) => {
            this.uploadedImages.update(images => [...images, {
              id: response.id,
              fileName: response.fileName,
              fileSize: response.fileSize
            }]);
            completed++;
            if (completed === imageFiles.length) {
              this.uploadingImages.set(false);
              this.snackBar.open(`${imageFiles.length} image(s) uploaded successfully`, 'Close', { duration: 3000 });
            }
          },
          error: (error) => {
            console.error('Error uploading image', error);
            completed++;
            if (completed === imageFiles.length) {
              this.uploadingImages.set(false);
            }
            this.snackBar.open(`Failed to upload ${file.name}`, 'Close', { duration: 3000 });
          }
        });
      };
      reader.readAsDataURL(file);
    });
  }

  /**
   * Removes an image from the upload list.
   */
  removeImage(imageId: number): void {
    this.uploadedImages.update(images => images.filter(img => img.id !== imageId));
    this.snackBar.open('Image removed', 'Close', { duration: 2000 });
  }

  /**
   * Formats file size for display.
   */
  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }
}
