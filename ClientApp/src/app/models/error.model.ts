/**
 * Standard error response from the API.
 */
export interface ErrorResponse {
  /** Error message describing what went wrong */
  message: string;
  /** Optional error code for client-side error handling */
  code?: string;
  /** Optional detailed error information */
  details?: any;
}
