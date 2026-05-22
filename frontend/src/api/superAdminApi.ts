import { apiClient } from './api-client';
import { GatewayResponse, PaginatedResponse } from '../types/common';
import { Tenant, TenantCredentials } from '../types/tenant';

export interface SystemDashboard {
  totalTenants: number;
  activeTenants: number;
  totalCustomers: number;
  activeSubscriptions: number;
  totalRevenue: number;
  failedPaymentsLast30Days: number;
  recentTenants: TenantSummary[];
}

export interface TenantSummary {
  id: string;
  name: string;
  isActive: boolean;
  createdAt: string;
}

export interface TenantRevenueBreakdown {
  tenantId: string;
  tenantName: string;
  totalRevenue: number;
  activeSubscriptions: number;
  totalCustomers: number;
}

export interface ImpersonationResponse {
  accessToken: string;
  tenantId: string;
  tenantName: string;
  expiresInMinutes: number;
}

export interface CreateTenantAdminRequest {
  email: string;
  password: string;
  firstName?: string;
  lastName?: string;
}

export interface PlatformSettings {
  platformName?: string;
  defaultCurrency?: string;
  maintenanceMode: boolean;
  defaultFeatures?: string;
  maxTenantsAllowed: number;
}

export const superAdminApi = {
  // Tenant CRUD
  getTenants: (page: number = 1, pageSize: number = 50, filters?: any) =>
    apiClient.get<PaginatedResponse<Tenant>>('/v1/super-admin/tenants', {
      params: { page, pageSize, ...filters },
    }),

  getTenant: (id: string) =>
    apiClient.get<GatewayResponse<Tenant>>(`/v1/super-admin/tenants/${id}`),

  createTenant: (data: any) =>
    apiClient.post<GatewayResponse<Tenant>>('/v1/super-admin/tenants', data),

  updateTenant: (id: string, data: any) =>
    apiClient.put<GatewayResponse<Tenant>>(`/v1/super-admin/tenants/${id}`, data),

  deleteTenant: (id: string) =>
    apiClient.delete<GatewayResponse<boolean>>(`/v1/super-admin/tenants/${id}`),

  getTenantCredentials: (id: string) =>
    apiClient.get<GatewayResponse<TenantCredentials>>(`/v1/super-admin/tenants/${id}/credentials`),

  generateNewCredentials: (id: string) =>
    apiClient.post<GatewayResponse<TenantCredentials>>(`/v1/super-admin/tenants/${id}/generate-credentials`, {}),

  toggleTenantStatus: (id: string) =>
    apiClient.post<GatewayResponse<Tenant>>(`/v1/super-admin/tenants/${id}/toggle-status`, {}),

  // Tenant Admin User
  createTenantAdmin: (tenantId: string, data: CreateTenantAdminRequest) =>
    apiClient.post<GatewayResponse<any>>(`/v1/super-admin/tenants/${tenantId}/admin-user`, data),

  // Impersonation
  impersonateTenant: (tenantId: string) =>
    apiClient.post<GatewayResponse<ImpersonationResponse>>(`/v1/super-admin/tenants/${tenantId}/impersonate`, {}),

  // Dashboard & Analytics
  getDashboard: () =>
    apiClient.get<GatewayResponse<SystemDashboard>>('/v1/super-admin/dashboard'),

  getTenantBreakdown: () =>
    apiClient.get<GatewayResponse<TenantRevenueBreakdown[]>>('/v1/super-admin/analytics/tenant-breakdown'),

  // Global Email Templates
  getGlobalEmailTemplates: () =>
    apiClient.get<GatewayResponse<any[]>>('/v1/super-admin/email-templates'),

  createGlobalEmailTemplate: (data: any) =>
    apiClient.post<GatewayResponse<any>>('/v1/super-admin/email-templates', data),

  updateGlobalEmailTemplate: (id: string, data: any) =>
    apiClient.put<GatewayResponse<any>>(`/v1/super-admin/email-templates/${id}`, data),

  deleteGlobalEmailTemplate: (id: string) =>
    apiClient.delete<GatewayResponse<boolean>>(`/v1/super-admin/email-templates/${id}`),

  // Platform Settings
  getPlatformSettings: () =>
    apiClient.get<GatewayResponse<PlatformSettings>>('/v1/super-admin/settings'),

  updatePlatformSettings: (data: Partial<PlatformSettings>) =>
    apiClient.put<GatewayResponse<PlatformSettings>>('/v1/super-admin/settings', data),
};
