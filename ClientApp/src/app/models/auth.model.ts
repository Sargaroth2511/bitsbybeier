// Re-export the generated UserRole enum for convenience
export { UserRole } from './generated/userrole.model';

/**
 * Represents a user in the system.
 */
export interface User {
  /** User's email address */
  email: string;
  /** User's display name */
  name: string;
  /** User's role in the system */
  role?: string;
  /** Unique identifier for the user */
  userId?: number;
}

/**
 * Response from authentication endpoint.
 */
export interface AuthenticationResponse {
  /** JWT token for authenticated requests */
  token: string;
  /** User's email address */
  email: string;
  /** User's display name */
  name: string;
  /** User's role in the system */
  role: string;
  /** Unique identifier for the user */
  userId: number;
}

/**
 * Request for Google token authentication.
 */
export interface GoogleTokenRequest {
  /** Google ID token from client-side authentication */
  idToken: string;
}
