export interface ConnectedAccount {
  id: string;
  stripeAccountId?: string;
  businessName?: string;
  email?: string;
  country?: string;
  type: string;
  chargesEnabled: boolean;
  payoutsEnabled: boolean;
  onboardingComplete: boolean;
  platformFeePercent: number;
  platformFeeFixed: number;
  createdAt: string;
}

export interface Transfer {
  id: string;
  connectedAccountId: string;
  stripeTransferId?: string;
  amount: number;
  currency: string;
  description?: string;
  status: string;
  createdAt: string;
}

export type ConnectedAccountResponse = ConnectedAccount;
export type TransferResponse = Transfer;

export interface PlatformBalance {
  available: number;
  pending: number;
  currency: string;
}
