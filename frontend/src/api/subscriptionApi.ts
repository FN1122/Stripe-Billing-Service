import { apiClient } from './api-client';
import { GatewayResponse, PaginatedResponse } from '../types/common';
import { Subscription, SubscriptionPlan, ProrationPreview } from '../types/subscription';

export const subscriptionApi = {
  getSubscriptions: (page: number = 1, pageSize: number = 50, filters?: any) =>
    apiClient.get<PaginatedResponse<Subscription>>('/v1/subscriptions', {
      params: { page, pageSize, ...filters },
    }),

  getSubscription: (id: string) =>
    apiClient.get<GatewayResponse<Subscription>>(`/v1/subscriptions/${id}`),

  createSubscription: (data: any) =>
    apiClient.post<GatewayResponse<Subscription>>('/v1/subscriptions', data),

  updateSubscription: (id: string, data: any) =>
    apiClient.put<GatewayResponse<Subscription>>(`/v1/subscriptions/${id}`, data),

  cancelSubscription: (id: string, data: { immediatelyCancelAtPeriodEnd?: boolean; reason: string }) =>
    apiClient.post<GatewayResponse<Subscription>>(`/v1/subscriptions/${id}/cancel`, data),

  getPlans: (isActive?: boolean) =>
    apiClient.get<GatewayResponse<SubscriptionPlan[]>>('/v1/subscriptions/plans', {
      params: { isActive },
    }),

  getPlan: (id: string) =>
    apiClient.get<GatewayResponse<SubscriptionPlan>>(`/v1/subscriptions/plans/${id}`),

  createPlan: (data: any) =>
    apiClient.post<GatewayResponse<SubscriptionPlan>>('/v1/subscriptions/plans', data),

  updatePlan: (id: string, data: any) =>
    apiClient.put<GatewayResponse<SubscriptionPlan>>(`/v1/subscriptions/plans/${id}`, data),

  previewProration: (subscriptionId: string, newPlanId: string) =>
    apiClient.get<GatewayResponse<ProrationPreview>>(`/v1/subscriptions/${subscriptionId}/proration-preview`, {
      params: { newPlanId },
    }),
};
