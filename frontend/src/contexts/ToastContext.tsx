import React, { createContext, useCallback } from 'react';
import { Toast } from 'react-toastify';
import { toast, ToastOptions } from 'react-toastify';

export interface ToastContextType {
  success: (message: string, options?: ToastOptions) => void;
  error: (message: string, options?: ToastOptions) => void;
  info: (message: string, options?: ToastOptions) => void;
  warning: (message: string, options?: ToastOptions) => void;
}

export const ToastContext = createContext<ToastContextType | undefined>(undefined);

export const ToastProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const defaultOptions: ToastOptions = {
    position: 'top-right',
    autoClose: 4000,
    hideProgressBar: false,
    closeOnClick: true,
    pauseOnHover: true,
    draggable: true,
  };

  const success = useCallback((message: string, options?: ToastOptions) => {
    toast.success(message, { ...defaultOptions, ...options });
  }, []);

  const error = useCallback((message: string, options?: ToastOptions) => {
    toast.error(message, { ...defaultOptions, ...options });
  }, []);

  const info = useCallback((message: string, options?: ToastOptions) => {
    toast.info(message, { ...defaultOptions, ...options });
  }, []);

  const warning = useCallback((message: string, options?: ToastOptions) => {
    toast.warning(message, { ...defaultOptions, ...options });
  }, []);

  const value: ToastContextType = {
    success,
    error,
    info,
    warning,
  };

  return <ToastContext.Provider value={value}>{children}</ToastContext.Provider>;
};
