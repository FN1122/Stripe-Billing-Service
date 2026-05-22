import React from 'react';
import './AuthLayout.scss';

interface AuthLayoutProps {
  children: React.ReactNode;
  title?: string;
}

export const AuthLayout: React.FC<AuthLayoutProps> = ({ children, title }) => {
  return (
    <main className="auth-layout">
      <div className="auth-center-wrapper">
        <div className="auth-card">
          <div className="text-center mb-4">
            <div className="auth-brand mb-4">
              <svg width="140" height="34" viewBox="0 0 140 34" fill="none" xmlns="http://www.w3.org/2000/svg">
                <rect width="34" height="34" rx="8" fill="#3572C6"/>
                <path d="M10 17h14M17 10v14" stroke="white" strokeWidth="2.5" strokeLinecap="round"/>
                <text x="42" y="24" fontFamily="Inter, sans-serif" fontSize="20" fontWeight="600" fill="#495057">Billing</text>
              </svg>
            </div>
            {title && (
              <>
                <h1 className="h2 auth-title">{title}</h1>
                <p className="text-tertiary">Sign in to your billing dashboard</p>
              </>
            )}
          </div>
          {children}
        </div>
        <div className="text-center text-muted small mt-4">
          © {new Date().getFullYear()} Stripe Billing Service
        </div>
      </div>
    </main>
  );
};
