/**
 * Defines user roles in the system for authorization purposes.
 * This enum matches the backend UserRole enum in Domain/Models/UserRole.cs
 * 
 * Note: This is also auto-generated in models/generated/userrole.model.ts
 * when running 'npm run generate-models', but we keep this definition here
 * to ensure the build works without requiring model generation first.
 */
export enum UserRole {
  /** Standard user with basic permissions */
  User = 'User',
  /** Administrator with full system access */
  Admin = 'Admin'
}

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
