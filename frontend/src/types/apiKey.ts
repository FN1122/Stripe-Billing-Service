export interface ApiKey {
  id: string;
  keyPrefix: string;
  plainKey: string | null;
  name: string;
  environment: string;
  permissions: string[];
  rateLimitPerMinute: number;
  isActive: boolean;
  lastUsedAt: string | null;
  expiresAt: string | null;
  totalRequests: number;
  createdAt: string;
}
