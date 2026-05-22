import { apiClient } from './api-client';
import { GatewayResponse, PaginatedResponse } from '../types/common';
import { WebhookSubscription, WebhookDelivery } from '../types/webhook';

export const webhookApi = {
  getSubscriptions: (page: number = 1, pageSize: number = 50) =>
    apiClient.get<PaginatedResponse<WebhookSubscription>>('/v1/webhooks/subscriptions', {
      params: { page, pageSize },
    }),

  getSubscription: (id: string) =>
    apiClient.get<GatewayResponse<WebhookSubscription>>(`/v1/webhooks/subscriptions/${id}`),

  createSubscription: (data: any) =>
    apiClient.post<GatewayResponse<WebhookSubscription>>('/v1/webhooks/subscriptions', data),

  updateSubscription: (id: string, data: any) =>
    apiClient.put<GatewayResponse<WebhookSubscription>>(`/v1/webhooks/subscriptions/${id}`, data),

  deleteSubscription: (id: string) =>
    apiClient.delete<GatewayResponse<boolean>>(`/v1/webhooks/subscriptions/${id}`),

  testDelivery: (id: string) =>
    apiClient.post<GatewayResponse<WebhookDelivery>>(`/v1/webhooks/subscriptions/${id}/test`, {}),

  getDeliveries: (subscriptionId: string, page: number = 1, pageSize: number = 50) =>
    apiClient.get<PaginatedResponse<WebhookDelivery>>('/v1/webhooks/deliveries', {
      params: { subscriptionId, page, pageSize },
    }),

  getDelivery: (id: string) =>
    apiClient.get<GatewayResponse<WebhookDelivery>>(`/v1/webhooks/deliveries/${id}`),

  retryDelivery: (id: string) =>
    apiClient.post<GatewayResponse<WebhookDelivery>>(`/v1/webhooks/deliveries/${id}/retry`, {}),
};
