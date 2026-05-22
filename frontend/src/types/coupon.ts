export interface Coupon {
  id: string;
  tenantId: string;
  stripeCouponId?: string;
  name: string;
  type: 'percent_off' | 'amount_off';
  amountOff?: number;
  percentOff?: number;
  currency?: string;
  duration: 'once' | 'repeating' | 'forever';
  durationInMonths?: number;
  maxRedemptions?: number;
  timesRedeemed: number;
  redeemBy?: string;
  isActive: boolean;
  promotionCodes: PromotionCode[];
  metadata?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface PromotionCode {
  id: string;
  couponId: string;
  stripePromotionCodeId?: string;
  code: string;
  isActive: boolean;
  maxRedemptions?: number;
  timesRedeemed: number;
  expiresAt?: string;
  firstTimeTransaction: boolean;
  minimumAmount?: number;
  minimumAmountCurrency?: string;
  createdAt: string;
}

export interface CouponRedemption {
  id: string;
  couponId: string;
  promotionCodeId?: string;
  customerId: string;
  subscriptionId?: string;
  customerName?: string;
  customerEmail?: string;
  amountDiscounted: number;
  currency: string;
  redeemedAt: string;
}

export interface CouponStats {
  totalCoupons: number;
  activeCoupons: number;
  totalRedemptions: number;
  totalDiscountAmount: number;
  mostUsedCouponName?: string;
  redemptionsByMonth: Record<string, number>;
}

export type CouponResponse = Coupon;
export type PromotionCodeResponse = PromotionCode;

export interface CouponFilter {
  page?: number;
  pageSize?: number;
  search?: string;
  type?: string;
  duration?: string;
  isActive?: boolean;
  sortBy?: string;
  sortDirection?: string;
  fromDate?: string;
  toDate?: string;
}
