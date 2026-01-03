/**
 * Represents an image associated with content, stored as a blob in the database.
 * Auto-generated - do not modify manually
 */
export interface ContentImage {
  id: number;
  imageData: byte[];
  contentType: string;
  fileName: string | null;
  /** Size of the image file in bytes. */
  fileSize: number;
  /** UTC timestamp when the image was uploaded. */
  uploadedAt: string;
  /** Foreign key to the associated content item. */
  contentId: number;
  /** Navigation property to the associated content item. */
  content: Content;
}
