/**
 * API endpoint constants for consistent URL management.
 */
export const API_ENDPOINTS = {
  /** Authentication endpoints */
  AUTH: {
    GOOGLE_LOGIN: '/api/auth/google',
    GET_USER: '/api/auth/user'
  },
  /** CMS endpoints */
  CMS: {
    ROOT: '/api/cms',
    CONTENT: '/api/cms/content',
    DRAFTS: '/api/cms/content/drafts',
    PUBLIC: '/api/cms/content/public'
  },
  /** Health check endpoint */
  HEALTH: '/api/health'
} as const;

/**
 * Storage keys for local storage.
 */
export const STORAGE_KEYS = {
  JWT_TOKEN: 'jwt_token'
} as const;
