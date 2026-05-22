import React, { createContext, useState, useCallback, useEffect } from 'react';
import { User, LoginRequest, ImpersonationState } from '../types/auth';
import { authApi } from '../api/authApi';
import { setAuthToken, setImpersonatingMode } from '../api/api-client';

export interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  isImpersonating: boolean;
  impersonatedTenantName: string | null;
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => Promise<void>;
  register: (data: any) => Promise<void>;
  changePassword: (data: any) => Promise<void>;
  clearError: () => void;
  startImpersonation: (token: string, tenantId: string, tenantName: string) => void;
  stopImpersonation: () => void;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [impersonation, setImpersonation] = useState<ImpersonationState>({
    isImpersonating: false,
    impersonatedTenantId: null,
    impersonatedTenantName: null,
    originalToken: null,
  });

  // Check if user is already logged in
  useEffect(() => {
    const initAuth = async () => {
      const token = localStorage.getItem('accessToken');
      if (token) {
        try {
          setAuthToken(token);
          const { data } = await authApi.getMe();
          if (data.isValid) {
            setUser(data.data);
          } else {
            localStorage.removeItem('accessToken');
            localStorage.removeItem('refreshToken');
          }
        } catch (err) {
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
        }
      }
      setIsLoading(false);
    };

    initAuth();
  }, []);

  const login = useCallback(async (credentials: LoginRequest) => {
    setIsLoading(true);
    setError(null);
    try {
      const { data } = await authApi.login(credentials);
      if (data.isValid) {
        localStorage.setItem('accessToken', data.data.accessToken);
        localStorage.setItem('refreshToken', data.data.refreshToken);
        setAuthToken(data.data.accessToken);
        setUser(data.data.user);
      } else {
        throw new Error(data.message || 'Login failed');
      }
    } catch (err: any) {
      const message = err.response?.data?.message || err.message || 'Login failed';
      setError(message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const logout = useCallback(async () => {
    setIsLoading(true);
    // If impersonating, just stop impersonation instead of full logout
    if (impersonation.isImpersonating && impersonation.originalToken) {
      setImpersonatingMode(false);
      setAuthToken(impersonation.originalToken);
      localStorage.setItem('accessToken', impersonation.originalToken);
      setImpersonation({
        isImpersonating: false,
        impersonatedTenantId: null,
        impersonatedTenantName: null,
        originalToken: null,
      });
      try {
        const { data } = await authApi.getMe();
        if (data.isValid) {
          setUser(data.data);
        }
      } catch {
        // fallback
      }
      setIsLoading(false);
      return;
    }
    try {
      await authApi.logout();
    } catch (err) {
      // Ignore errors on logout
    } finally {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      setAuthToken(null);
      setUser(null);
      setIsLoading(false);
    }
  }, [impersonation]);

  const register = useCallback(async (data: any) => {
    setIsLoading(true);
    setError(null);
    try {
      const { data: response } = await authApi.register(data);
      if (response.isValid) {
        localStorage.setItem('accessToken', response.data.accessToken);
        localStorage.setItem('refreshToken', response.data.refreshToken);
        setAuthToken(response.data.accessToken);
        setUser(response.data.user);
      } else {
        throw new Error(response.message || 'Registration failed');
      }
    } catch (err: any) {
      const message = err.response?.data?.message || err.message || 'Registration failed';
      setError(message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const changePassword = useCallback(async (data: any) => {
    setIsLoading(true);
    setError(null);
    try {
      const { data: response } = await authApi.changePassword(data);
      if (!response.isValid) {
        throw new Error(response.message || 'Password change failed');
      }
    } catch (err: any) {
      const message = err.response?.data?.message || err.message || 'Password change failed';
      setError(message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  const startImpersonation = useCallback((token: string, tenantId: string, tenantName: string) => {
    const originalToken = localStorage.getItem('accessToken');
    setImpersonation({
      isImpersonating: true,
      impersonatedTenantId: tenantId,
      impersonatedTenantName: tenantName,
      originalToken,
    });
    localStorage.setItem('accessToken', token);
    setAuthToken(token);
    setImpersonatingMode(true);
    setUser(prev => prev ? {
      ...prev,
      tenantId,
      role: 'Viewer',
    } : null);
  }, []);

  const stopImpersonation = useCallback(() => {
    setImpersonatingMode(false);
    if (impersonation.originalToken) {
      localStorage.setItem('accessToken', impersonation.originalToken);
      setAuthToken(impersonation.originalToken);
      authApi.getMe().then(({ data }) => {
        if (data.isValid) {
          setUser(data.data);
        }
      });
    }
    setImpersonation({
      isImpersonating: false,
      impersonatedTenantId: null,
      impersonatedTenantName: null,
      originalToken: null,
    });
  }, [impersonation.originalToken]);

  const value: AuthContextType = {
    user,
    isAuthenticated: !!user,
    isLoading,
    error,
    isImpersonating: impersonation.isImpersonating,
    impersonatedTenantName: impersonation.impersonatedTenantName,
    login,
    logout,
    register,
    changePassword,
    clearError,
    startImpersonation,
    stopImpersonation,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
