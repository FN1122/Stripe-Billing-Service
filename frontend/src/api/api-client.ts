import axios from 'axios';

const baseURL = import.meta.env.VITE_API_URL || 'https://localhost:58492/api';

export const apiClient = axios.create({
  baseURL,
  headers: { 'Content-Type': 'application/json' },
});

let _isImpersonating = false;

export const setImpersonatingMode = (impersonating: boolean) => {
  _isImpersonating = impersonating;
};

// Block non-GET requests when impersonating (read-only mode)
apiClient.interceptors.request.use((config) => {
  if (_isImpersonating && config.method && config.method.toLowerCase() !== 'get') {
    // Allow super-admin endpoints (for stopImpersonation flow) but block tenant-scoped writes
    const url = config.url || '';
    if (!url.includes('/super-admin/')) {
      return Promise.reject(new Error('Read-only mode: modifications are not allowed while impersonating a tenant.'));
    }
  }
  return config;
});

export const setAuthToken = (token: string | null) => {
  if (token) {
    apiClient.defaults.headers.common['Authorization'] = `Bearer ${token}`;
  } else {
    delete apiClient.defaults.headers.common['Authorization'];
  }
};
