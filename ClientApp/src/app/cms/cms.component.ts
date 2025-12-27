import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../services/auth.service';
import { User } from '../models/auth.model';
import { CmsContent } from '../models/cms.model';
import { API_ENDPOINTS } from '../constants/api.constants';

/**
 * Component for the Content Management System (CMS) interface.
 * Accessible only to users with Admin role.
 */
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

  /**
   * Initializes the component by subscribing to current user and loading content.
   */
  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
    
    this.loadContent();
  }

  /**
   * Loads CMS content from the API.
   */
  loadContent(): void {
    this.loading = true;
    this.error = '';
    
    this.http.get<CmsContent[]>(API_ENDPOINTS.CMS.CONTENT).subscribe(
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
