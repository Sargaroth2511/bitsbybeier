import { Component, OnInit } from '@angular/core';
import { CmsService } from '../services/cms.service';
import { CmsContent } from '../models/cms.model';

/**
 * Component for displaying public blog content.
 */
@Component({
    selector: 'app-blog',
    templateUrl: './blog.component.html',
    styleUrls: ['./blog.component.scss'],
    standalone: false
})
export class BlogComponent implements OnInit {
  posts: CmsContent[] = [];
  loading = false;
  error = '';

  constructor(private cmsService: CmsService) {}

  ngOnInit(): void {
    this.loadPosts();
  }

  /**
   * Loads public blog posts from the API.
   */
  loadPosts(): void {
    this.loading = true;
    this.error = '';
    
    this.cmsService.getPublicContent().subscribe({
      next: (data) => {
        this.posts = data;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load blog posts';
        this.loading = false;
        console.error('Error loading blog posts', error);
      }
    });
  }

  /**
   * Gets preview text for a post.
   */
  getPreviewText(content: string, maxLength: number = 300): string {
    const text = content
      .replace(/#+\s/g, '') // Remove headers
      .replace(/\*\*/g, '') // Remove bold
      .replace(/\*/g, '') // Remove italic
      .replace(/\[([^\]]+)\]\([^\)]+\)/g, '$1'); // Remove links
    
    return text.length > maxLength ? text.substring(0, maxLength) + '...' : text;
  }
}
