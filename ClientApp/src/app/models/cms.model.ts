/**
 * Represents a content item in the CMS.
 */
export interface CmsContent {
  /** Unique identifier for the content */
  id: number;
  /** Content title */
  title: string;
  /** Content body (optional) */
  body?: string;
  /** ISO 8601 timestamp when content was created */
  createdAt: string;
}

/**
 * Request for creating new content.
 */
export interface ContentRequest {
  /** Title of the content (required) */
  title: string;
  /** Body content (optional) */
  body?: string;
}
