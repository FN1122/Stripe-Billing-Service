import { Subscription } from './subscription';
import { PaymentTransaction } from './payment';
import { Invoice } from './invoice';

export interface Customer {
  id: string;
  externalReferenceId: string;
  stripeCustomerId: string;
  email: string;
  name: string;
  phone: string;
  currency: string;
  subscriptionCount: number;
  totalSpent: number;
  createdAt: string;
}

export interface CustomerDetail extends Customer {
  billingAddress: string;
  taxId: string;
  subscriptions: Subscription[];
  recentTransactions: PaymentTransaction[];
  invoices: Invoice[];
}
