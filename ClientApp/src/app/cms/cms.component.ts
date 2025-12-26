import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService, UserInfo } from '../services/auth.service';
import { environment } from '../../environments/environment';

interface Content {
  id: number;
  title: string;
  body: string;
}

interface CmsContentResponse {
  message: string;
  user: UserInfo;
  content: Content[];
}

@Component({
  selector: 'app-cms',
  templateUrl: './cms.component.html',
  standalone: false
})
export class CmsComponent implements OnInit {
  user: UserInfo | null = null;
  content: Content[] = [];
  loading = false;
  error: string | null = null;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.authService.user$.subscribe(user => {
      this.user = user;
    });
    this.loadContent();
  }

  loadContent(): void {
    this.loading = true;
    this.error = null;

    this.http.get<CmsContentResponse>(`${environment.apiUrl}/api/cms/content`)
      .subscribe({
        next: (response) => {
          this.content = response.content;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading content:', error);
          this.error = 'Failed to load content';
          this.loading = false;
        }
      });
  }

  logout(): void {
    this.authService.logout();
  }
}
