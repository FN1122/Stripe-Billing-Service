import { apiClient } from './api-client';
import { GatewayResponse, PaginatedResponse } from '../types/common';
import { PaymentTransaction, CreateCheckoutRequest, PaymentAnalytics } from '../types/payment';

export const paymentApi = {
  getTransactions: (page: number = 1, pageSize: number = 50, filters?: any) =>
    apiClient.get<PaginatedResponse<PaymentTransaction>>('/v1/payments', {
      params: { page, pageSize, ...filters },
    }),

  getTransaction: (id: string) =>
    apiClient.get<GatewayResponse<PaymentTransaction>>(`/v1/payments/${id}`),

  getAnalytics: (startDate?: string, endDate?: string) =>
    apiClient.get<GatewayResponse<PaymentAnalytics>>('/v1/payments/analytics', {
      params: { startDate, endDate },
    }),

  createCheckout: (data: CreateCheckoutRequest) =>
    apiClient.post<GatewayResponse<{ checkoutUrl: string }>>('/v1/payments/checkout', data),

  refundTransaction: (id: string, data: { amount?: number; reason: string }) =>
    apiClient.post<GatewayResponse<boolean>>(`/v1/refunds`, { ...data, transactionId: id }),
};
