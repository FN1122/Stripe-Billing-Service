import { apiClient } from './api-client';
import { GatewayResponse, PaginatedResponse } from '../types/common';
import { WebhookEvent, WebhookEventDetail, WebhookDelivery, WebhookDeliveryDetail, WebhookEventStats } from '../types/webhookEvent';

export const webhookEventApi = {
  getInboundEvents: (params?: any) => apiClient.get<PaginatedResponse<WebhookEvent>>('/v1/webhook-events/inbound', { params }),
  getInboundEvent: (id: string) => apiClient.get<GatewayResponse<WebhookEventDetail>>(`/v1/webhook-events/inbound/${id}`),
  replayEvent: (id: string) => apiClient.post<GatewayResponse<boolean>>(`/v1/webhook-events/inbound/${id}/replay`),
  getDeliveries: (params?: any) => apiClient.get<PaginatedResponse<WebhookDelivery>>('/v1/webhook-events/deliveries', { params }),
  getDelivery: (id: string) => apiClient.get<GatewayResponse<WebhookDeliveryDetail>>(`/v1/webhook-events/deliveries/${id}`),
  retryDelivery: (id: string) => apiClient.post<GatewayResponse<boolean>>(`/v1/webhook-events/deliveries/${id}/retry`),
  getStats: () => apiClient.get<GatewayResponse<WebhookEventStats>>('/v1/webhook-events/stats'),
};
