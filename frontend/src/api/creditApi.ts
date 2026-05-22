import { apiClient } from './api-client';
import { GatewayResponse, PaginatedResponse } from '../types/common';
import { CustomerBalance, CreditTransaction, CreditsDashboard } from '../types/credit';

export const creditApi = {
  getBalance: (customerId: string) => apiClient.get<GatewayResponse<CustomerBalance>>(`/v1/credits/customers/${customerId}/balance`),
  addCredit: (customerId: string, data: any) => apiClient.post<GatewayResponse<CreditTransaction>>(`/v1/credits/customers/${customerId}/credit`, data),
  adjustBalance: (customerId: string, data: any) => apiClient.post<GatewayResponse<CreditTransaction>>(`/v1/credits/customers/${customerId}/adjust`, data),
  getHistory: (customerId: string, page?: number, pageSize?: number) => apiClient.get<PaginatedResponse<CreditTransaction>>(`/v1/credits/customers/${customerId}/history`, { params: { page, pageSize } }),
  refundToCredit: (data: any) => apiClient.post<GatewayResponse<CreditTransaction>>('/v1/credits/refund-to-credit', data),
  getDashboard: () => apiClient.get<GatewayResponse<CreditsDashboard>>('/v1/credits/dashboard'),
  getTransactions: (page?: number, pageSize?: number) => apiClient.get<PaginatedResponse<CreditTransaction>>('/v1/credits/transactions', { params: { page, pageSize } }),
};
