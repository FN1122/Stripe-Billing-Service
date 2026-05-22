export interface WebhookEvent {
  id: string;
  stripeEventId?: string;
  eventType: string;
  status: string;
  retryCount?: number;
  processedAt?: string;
  createdAt: string;
  payload?: string;
}

export interface WebhookEventDetail extends WebhookEvent {
  payloadJson: string;
}

export interface WebhookDelivery {
  id: string;
  webhookSubscriptionId: string;
  eventType: string;
  status: string;
  httpStatusCode?: number;
  attempts: number;
  lastAttemptedAt?: string;
  nextRetryAt?: string;
}

export interface WebhookDeliveryDetail extends WebhookDelivery {
  payloadJson: string;
  responseBody?: string;
}

export interface WebhookEventStats {
  totalEventsReceived: number;
  totalEventsProcessed: number;
  totalEventsFailed: number;
  totalDeliveriesSucceeded: number;
  totalDeliveriesFailed: number;
  eventsByType: Record<string, number>;
}
