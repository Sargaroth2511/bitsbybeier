/**
 * Represents a content item in the CMS with support for drafts and publication workflow.
 * Auto-generated - do not modify manually
 */
export interface Content {
  id: number;
  author: string;
  /** UTC timestamp when the content was created. */
  createdAt: string;
  /** UTC timestamp when the content was last updated. */
  updatedAt: string | null;
  /** Indicates whether the content is active. */
  active: boolean;
  /** Indicates whether the content is in draft state. Draft content is not published and can be used for preview purposes. */
  draft: boolean;
  title: string;
  subtitle: string | null;
  contentText: string;
  /** UTC timestamp when the content should be published. If null, content can be published immediately when Draft is set to false. */
  publishAt: string | null;
  /** Navigation property for associated images. */
  images: ContentImage[];
}
