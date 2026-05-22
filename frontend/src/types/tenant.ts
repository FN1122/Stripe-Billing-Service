export interface Tenant {
  id: string;
  name: string;
  slug: string;
  plan: string;
  isActive: boolean;
  totalRevenue: number;
  activeSubscriptions: number;
  totalCustomers: number;
  createdAt: string;
}

export interface TenantCredentials {
  tenantId: string;
  publicApiKey: string;
  secretApiKey: string;
  webhookSigningSecret: string;
  jwtSigningSecret: string;
  message: string;
}
