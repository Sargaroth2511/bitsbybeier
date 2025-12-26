import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService, User } from '../services/auth.service';

interface CmsContent {
  id: number;
  title: string;
  createdAt: string;
}

@Component({
    selector: 'app-cms',
    templateUrl: './cms.component.html',
    standalone: false
})
export class CmsComponent implements OnInit {
  currentUser: User | null = null;
  content: CmsContent[] = [];
  loading = false;
  error = '';

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
    
    this.loadContent();
  }

  loadContent(): void {
    this.loading = true;
    this.error = '';
    
    this.http.get<CmsContent[]>('/api/cms/content').subscribe(
      data => {
        this.content = data;
        this.loading = false;
      },
      error => {
        this.error = 'Failed to load content';
        this.loading = false;
        console.error('Error loading content', error);
      }
    );
  }
}
