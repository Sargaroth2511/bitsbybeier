import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CmsContent, ContentUpdateRequest } from '../models/cms.model';
import { API_ENDPOINTS } from '../constants/api.constants';

/**
 * Service for CMS operations.
 */
@Injectable({
  providedIn: 'root'
})
export class CmsService {
  constructor(private http: HttpClient) {}

  /**
   * Gets all content items.
   */
  getAllContent(): Observable<CmsContent[]> {
    return this.http.get<CmsContent[]>(API_ENDPOINTS.CMS.CONTENT);
  }

  /**
   * Gets draft content items only.
   */
  getDraftContent(): Observable<CmsContent[]> {
    return this.http.get<CmsContent[]>(API_ENDPOINTS.CMS.DRAFTS);
  }

  /**
   * Gets public (published) content items.
   */
  getPublicContent(): Observable<CmsContent[]> {
    return this.http.get<CmsContent[]>(API_ENDPOINTS.CMS.PUBLIC);
  }

  /**
   * Updates a content item's status.
   */
  updateContent(id: number, request: ContentUpdateRequest): Observable<CmsContent> {
    return this.http.put<CmsContent>(`${API_ENDPOINTS.CMS.CONTENT}/${id}`, request);
  }

  /**
   * Deletes a content item.
   */
  deleteContent(id: number): Observable<void> {
    return this.http.delete<void>(`${API_ENDPOINTS.CMS.CONTENT}/${id}`);
  }
}
