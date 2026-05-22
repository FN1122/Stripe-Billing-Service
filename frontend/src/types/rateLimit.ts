export interface RateLimit {
  id: string;
  endpoint: string;
  requestsPerMinute: number;
  burstLimit?: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface RateLimitUsage {
  endpoint: string;
  currentRequests: number;
  limit: number;
  remaining: number;
  resetsAt: string;
}
