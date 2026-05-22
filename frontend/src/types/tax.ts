export interface TaxConfiguration {
  id: string;
  tenantId: string;
  taxProvider: string;
  automaticTax: boolean;
  defaultTaxBehavior: string;
  taxRegistrations: TaxRegistration[];
  createdAt: string;
  updatedAt?: string;
}

export interface TaxRegistration {
  country: string;
  state?: string;
  taxId: string;
  type: string;
}

export interface TaxCalculationPreview {
  subtotal: number;
  taxAmount: number;
  total: number;
  taxBreakdown: TaxLineItem[];
}

export interface TaxLineItem {
  jurisdiction: string;
  taxRate: number;
  taxableAmount: number;
  taxAmount: number;
  description: string;
}

export interface TaxReport {
  periodFrom: string;
  periodTo: string;
  totalTaxCollected: number;
  taxableRevenue: number;
  exemptRevenue: number;
  byJurisdiction: Record<string, number>;
}

export interface TaxRate {
  country: string;
  state?: string;
  rate: number;
  description: string;
  inclusive: boolean;
}

export interface TaxConfigurationResponse {
  provider: string;
  isEnabled: boolean;
  autoCalculate: boolean;
  defaultTaxBehavior: string;
  fallbackTaxRate?: number;
  registrationNumbers?: Array<{ country: string; type: string; value: string }>;
  updatedAt?: string;
}
