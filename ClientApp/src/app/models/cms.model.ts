/**
 * Represents a content item in the CMS.
 */
export interface CmsContent {
  /** Unique identifier for the content */
  id: number;
  /** Author of the content */
  author: string;
  /** Content title */
  title: string;
  /** Content subtitle */
  subtitle?: string;
  /** Content body text (supports Markdown) */
  content: string;
  /** Whether content is in draft state */
  draft: boolean;
  /** Whether content is active */
  active: boolean;
  /** ISO 8601 timestamp when content was created */
  createdAt: string;
  /** ISO 8601 timestamp when content was last updated */
  updatedAt?: string;
  /** ISO 8601 timestamp when content should be published */
  publishAt?: string;
}

/**
 * Request for creating new content.
 */
export interface ContentRequest {
  /** Author name (required) */
  author: string;
  /** Title of the content (required) */
  title: string;
  /** Subtitle (optional) */
  subtitle?: string;
  /** Body content (required) */
  content: string;
  /** Whether to create as draft (default: true) */
  draft?: boolean;
}

/**
 * Request for updating content status.
 */
export interface ContentUpdateRequest {
  /** Whether content should be in draft state */
  draft?: boolean;
  /** Whether content is active */
  active?: boolean;
  /** Timestamp when content should be published */
  publishAt?: string;
}

/**
 * Request for full content update.
 */
export interface ContentFullUpdateRequest {
  /** Author name */
  author?: string;
  /** Title of the content */
  title?: string;
  /** Subtitle */
  subtitle?: string;
  /** Body content */
  content?: string;
  /** Whether content should be in draft state */
  draft?: boolean;
  /** Whether content is active */
  active?: boolean;
  /** Timestamp when content should be published */
  publishAt?: string;
}
