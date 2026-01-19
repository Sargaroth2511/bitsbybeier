import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CmsService } from '../services/cms.service';
import { CmsContent } from '../models/cms.model';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner.component';
import { ErrorDisplayComponent } from '../shared/components/error-display.component';
import { MarkdownPipe } from '../shared/pipes/markdown.pipe';
import { DatePipe } from '@angular/common';

/**
 * Component for displaying a single blog post.
 */
@Component({
  selector: 'app-blog-detail',
  templateUrl: './blog-detail.component.html',
  styleUrls: ['./blog-detail.component.scss'],
  standalone: true,
  imports: [
    MatIconModule,
    MatButtonModule,
    LoadingSpinnerComponent,
    ErrorDisplayComponent,
    MarkdownPipe,
    DatePipe
  ]
})
export class BlogDetailComponent implements OnInit {
  post = signal<CmsContent | null>(null);
  loading = signal(false);
  error = signal('');

  constructor(
    private cmsService: CmsService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const id = +params['id'];
      if (id) {
        this.loadPost(id);
      }
    });
  }

  /**
   * Loads a single blog post from the API.
   */
  loadPost(id: number): void {
    this.loading.set(true);
    this.error.set('');
    
    this.cmsService.getPublicContentById(id).subscribe({
      next: (data) => {
        this.post.set(data);
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set('Failed to load blog post');
        this.loading.set(false);
        console.error('Error loading blog post', error);
      }
    });
  }

  /**
   * Navigate back to blog list.
   */
  goBack(): void {
    this.router.navigate(['/blog']);
  }
}
