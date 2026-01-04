/**
 * Represents a user profile image stored in the database.
 * Auto-generated - do not modify manually
 */
export interface UserImage {
  id: number;
  imageData: byte[];
  contentType: string;
  fileName: string | null;
  /** Size of the image file in bytes. */
  fileSize: number;
  /** UTC timestamp when the image was uploaded. */
  uploadedAt: string;
}
