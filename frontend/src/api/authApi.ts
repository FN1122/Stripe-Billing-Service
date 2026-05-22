import { apiClient } from './api-client';
import { GatewayResponse } from '../types/common';
import { LoginRequest, LoginResponse, User, RegisterRequest, ChangePasswordRequest } from '../types/auth';

export const authApi = {
  login: (data: LoginRequest) =>
    apiClient.post<GatewayResponse<LoginResponse>>('/v1/auth/login', data),

  register: (data: RegisterRequest) =>
    apiClient.post<GatewayResponse<LoginResponse>>('/v1/auth/register', data),

  refreshToken: (data: { accessToken: string; refreshToken: string }) =>
    apiClient.post<GatewayResponse<LoginResponse>>('/v1/auth/refresh-token', data),

  getMe: () =>
    apiClient.get<GatewayResponse<User>>('/v1/auth/me'),

  changePassword: (data: ChangePasswordRequest) =>
    apiClient.post<GatewayResponse<boolean>>('/v1/auth/change-password', data),

  logout: () =>
    apiClient.post<GatewayResponse<boolean>>('/v1/auth/logout', {}),
};
