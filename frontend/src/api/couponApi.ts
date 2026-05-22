import { apiClient } from './api-client';
import { GatewayResponse, PaginatedResponse } from '../types/common';
import { Coupon, PromotionCode, CouponRedemption, CouponStats, CouponFilter } from '../types/coupon';

export const couponApi = {
  createCoupon: (data: any) => apiClient.post<GatewayResponse<Coupon>>('/v1/coupons', data),
  getCoupons: (filter?: CouponFilter) => apiClient.get<PaginatedResponse<Coupon>>('/v1/coupons', { params: filter }),
  getCoupon: (id: string) => apiClient.get<GatewayResponse<Coupon>>(`/v1/coupons/${id}`),
  updateCoupon: (id: string, data: any) => apiClient.put<GatewayResponse<Coupon>>(`/v1/coupons/${id}`, data),
  toggleCoupon: (id: string) => apiClient.post<GatewayResponse<Coupon>>(`/v1/coupons/${id}/toggle`),
  deleteCoupon: (id: string) => apiClient.delete<GatewayResponse<boolean>>(`/v1/coupons/${id}`),
  createPromotionCode: (couponId: string, data: any) => apiClient.post<GatewayResponse<PromotionCode>>(`/v1/coupons/${couponId}/promotion-codes`, data),
  getPromotionCodes: (couponId: string) => apiClient.get<GatewayResponse<PromotionCode[]>>(`/v1/coupons/${couponId}/promotion-codes`),
  deactivatePromotionCode: (id: string) => apiClient.post<GatewayResponse<boolean>>(`/v1/coupons/promotion-codes/${id}/deactivate`),
  validateCode: (code: string) => apiClient.post<GatewayResponse<Coupon>>('/v1/coupons/validate', { code }),
  applyCoupon: (subscriptionId: string, code: string) => apiClient.post<GatewayResponse<boolean>>('/v1/coupons/apply', { subscriptionId, code }),
  removeCoupon: (subscriptionId: string) => apiClient.post<GatewayResponse<boolean>>('/v1/coupons/remove', { subscriptionId }),
  getRedemptions: (couponId: string) => apiClient.get<GatewayResponse<CouponRedemption[]>>(`/v1/coupons/${couponId}/redemptions`),
  getStats: () => apiClient.get<GatewayResponse<CouponStats>>('/v1/coupons/stats'),
};
