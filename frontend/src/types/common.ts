export interface GatewayResponse<T> {
  isValid: boolean;
  success: boolean;
  message: string;
  data: T;
  errors: string[];
  statusCode: number;
}

export interface PaginatedResponse<T> extends GatewayResponse<T[]> {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
