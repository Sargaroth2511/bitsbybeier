import { Component, OnInit } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
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
  posts: (CmsContent & { expanded?: boolean })[] = [];
  loading = false;
  error = '';

  constructor(
    private cmsService: CmsService,
    private sanitizer: DomSanitizer
  ) {}

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
        this.posts = data.map(post => ({ ...post, expanded: false }));
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
   * Toggles the expanded state of a post.
   */
  toggleExpanded(post: CmsContent & { expanded?: boolean }): void {
    post.expanded = !post.expanded;
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
}
