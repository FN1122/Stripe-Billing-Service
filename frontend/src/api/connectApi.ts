import { apiClient } from './api-client';
import { GatewayResponse } from '../types/common';
import { ConnectedAccount, Transfer, PlatformBalance } from '../types/connect';

export const connectApi = {
  createAccount: (data: any) => apiClient.post<GatewayResponse<ConnectedAccount>>('/v1/connect/accounts', data),
  getAccounts: () => apiClient.get<GatewayResponse<ConnectedAccount[]>>('/v1/connect/accounts'),
  getAccount: (id: string) => apiClient.get<GatewayResponse<ConnectedAccount>>(`/v1/connect/accounts/${id}`),
  getOnboardingLink: (id: string) => apiClient.post<GatewayResponse<string>>(`/v1/connect/accounts/${id}/onboarding-link`),
  getDashboardLink: (id: string) => apiClient.post<GatewayResponse<string>>(`/v1/connect/accounts/${id}/dashboard-link`),
  createTransfer: (data: any) => apiClient.post<GatewayResponse<Transfer>>('/v1/connect/transfers', data),
  getTransfers: () => apiClient.get<GatewayResponse<Transfer[]>>('/v1/connect/transfers'),
  getBalance: () => apiClient.get<GatewayResponse<PlatformBalance>>('/v1/connect/balance'),
};
