import { apiClient } from './api-client';
import { GatewayResponse } from '../types/common';
import { TaxConfiguration, TaxCalculationPreview, TaxReport, TaxRate } from '../types/tax';

export const taxApi = {
  getConfig: () => apiClient.get<GatewayResponse<TaxConfiguration>>('/v1/tax/config'),
  getConfiguration: () => apiClient.get<GatewayResponse<TaxConfiguration>>('/v1/tax/config'),
  updateConfig: (data: any) => apiClient.put<GatewayResponse<TaxConfiguration>>('/v1/tax/config', data),
  updateConfiguration: (data: any) => apiClient.put<GatewayResponse<TaxConfiguration>>('/v1/tax/config', data),
  previewTax: (data: any) => apiClient.post<GatewayResponse<TaxCalculationPreview>>('/v1/tax/preview', data),
  setCustomerExempt: (customerId: string, data: any) => apiClient.post<GatewayResponse<boolean>>(`/v1/tax/customers/${customerId}/exempt`, data),
  addTaxId: (customerId: string, data: any) => apiClient.post<GatewayResponse<boolean>>(`/v1/tax/customers/${customerId}/tax-ids`, data),
  removeTaxId: (customerId: string, taxIdId: string) => apiClient.delete<GatewayResponse<boolean>>(`/v1/tax/customers/${customerId}/tax-ids/${taxIdId}`),
  getTaxReport: (from: string, to: string) => apiClient.get<GatewayResponse<TaxReport>>('/v1/tax/report', { params: { from, to } }),
  getTaxRates: (country: string) => apiClient.get<GatewayResponse<TaxRate[]>>('/v1/tax/rates', { params: { country } }),
};
