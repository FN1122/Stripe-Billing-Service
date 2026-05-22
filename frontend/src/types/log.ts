export interface LogEntry {
  id: string;
  serviceType: string;
  endpoint: string;
  method: string;
  responseStatusCode: number;
  durationMs: number;
  status: string;
  ipAddress: string;
  requestBody: string;
  responseBody: string;
  createdAt: string;
}

export interface LogStats {
  totalCalls: number;
  successCount: number;
  errorCount: number;
  avgDurationMs: number;
  successRate: number;
}
