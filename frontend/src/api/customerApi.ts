import { apiClient } from './api-client';
import { GatewayResponse, PaginatedResponse } from '../types/common';
import { Customer, CustomerDetail } from '../types/customer';

export const customerApi = {
  getCustomers: (page: number = 1, pageSize: number = 50, search?: string) =>
    apiClient.get<PaginatedResponse<Customer>>('/v1/customers', {
      params: { page, pageSize, search },
    }),

  getCustomer: (id: string) =>
    apiClient.get<GatewayResponse<CustomerDetail>>(`/v1/customers/${id}`),

  createCustomer: (data: any) =>
    apiClient.post<GatewayResponse<Customer>>('/v1/customers', data),

  updateCustomer: (id: string, data: any) =>
    apiClient.put<GatewayResponse<Customer>>(`/v1/customers/${id}`, data),

  deleteCustomer: (id: string) =>
    apiClient.delete<GatewayResponse<boolean>>(`/v1/customers/${id}`),

  getByEmail: (email: string) =>
    apiClient.get<GatewayResponse<Customer>>('/v1/customers/email', {
      params: { email },
    }),
};
