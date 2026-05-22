export interface WebhookSubscription {
  id: string;
  webhookUrl: string;
  events: string[];
  customHeaders: Record<string, string>;
  isActive: boolean;
  description: string;
  createdAt: string;
}

export interface WebhookDelivery {
  id: string;
  webhookSubscriptionId: string;
  eventType: string;
  payload: string;
  status: string;
  httpStatusCode: number | null;
  responseBody: string;
  durationMs: number | null;
  retryCount: number;
  failureReason: string;
  deliveredAt: string | null;
  createdAt: string;
}
