import { Component, OnInit, signal } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { CmsService } from '../services/cms.service';
import { CmsContent } from '../models/cms.model';
import { BlogPostCardComponent } from '../shared/components/blog-post-card.component';
import { LoadingSpinnerComponent } from '../shared/components/loading-spinner.component';
import { ErrorDisplayComponent } from '../shared/components/error-display.component';
import { MarkdownPipe } from '../shared/pipes/markdown.pipe';

/**
 * Component for displaying public blog content.
 */
@Component({
  selector: 'app-blog',
  templateUrl: './blog.component.html',
  styleUrls: ['./blog.component.scss'],
  standalone: true,
  imports: [
    MatIconModule,
    BlogPostCardComponent,
    LoadingSpinnerComponent,
    ErrorDisplayComponent,
    MarkdownPipe
  ]
})
export class BlogComponent implements OnInit {
  posts = signal<(CmsContent & { expanded?: boolean })[]>([]);
  loading = signal(false);
  error = signal('');

  constructor(private cmsService: CmsService) {}

  ngOnInit(): void {
    this.loadPosts();
  }

  /**
   * Loads public blog posts from the API.
   */
  loadPosts(): void {
    this.loading.set(true);
    this.error.set('');
    
    this.cmsService.getPublicContent().subscribe({
      next: (data) => {
        this.posts.set(data.map(post => ({ ...post, expanded: false })));
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set('Failed to load blog posts');
        this.loading.set(false);
        console.error('Error loading blog posts', error);
      }
    });
  }

  /**
   * Toggles the expanded state of a post.
   */
  toggleExpanded(index: number): void {
    this.posts.update(posts => {
      const newPosts = [...posts];
      newPosts[index] = { ...newPosts[index], expanded: !newPosts[index].expanded };
      return newPosts;
    });
  }
}
