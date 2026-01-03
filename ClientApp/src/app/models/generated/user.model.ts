/**
 * Represents a user in the system with authentication and profile information.
 * Auto-generated - do not modify manually
 */
export interface User {
  id: number;
  email: string;
  displayName: string;
  googleId: string | null;
  /** Foreign key to the user's profile image. */
  profileImageId: number | null;
  /** Navigation property to the user's profile image. */
  profileImage: UserImage | null;
  /** UTC timestamp when the user account was created. */
  createdAt: string;
  /** UTC timestamp of the user's last successful login. */
  lastLoginAt: string | null;
  /** Indicates whether the user account is active and can authenticate. */
  isActive: boolean;
  /** Indicates whether the user account has been soft-deleted. */
  isDeleted: boolean;
  /** User's role determining access permissions. */
  role: UserRole;
}
