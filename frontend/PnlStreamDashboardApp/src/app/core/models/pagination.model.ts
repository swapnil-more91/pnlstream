/**
 * Pagination request parameters
 */
export interface PaginationRequest {
  page: number;           // 0-based page index
  pageSize: number;       // Records per page
  sortBy?: string;        // Field to sort by (e.g., "sourceSystem", "pnlAmount")
  sortOrder?: 'ASC' | 'DESC';  // Sort direction
}

/**
 * Paginated response wrapper
 */
export interface PaginatedResponse<T> {
  data: T[];              // Array of items for current page
  totalRecords: number;   // Total number of records matching filter
  totalPages: number;     // Total pages available
  currentPage: number;    // Current page index (0-based)
  pageSize: number;       // Records per page
  hasNext: boolean;       // Whether there's a next page
  hasPrevious: boolean;   // Whether there's a previous page
}

/**
 * Pagination state management
 */
export interface PaginationState {
  pageIndex: number;
  pageSize: number;
  sortBy?: string;
  sortOrder?: 'ASC' | 'DESC';
}
