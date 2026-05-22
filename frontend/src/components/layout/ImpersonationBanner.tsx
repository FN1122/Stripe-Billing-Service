import React from 'react';
import { useAuth } from '../../hooks/useAuth';

export const ImpersonationBanner: React.FC = () => {
  const { isImpersonating, impersonatedTenantName, stopImpersonation } = useAuth();

  if (!isImpersonating) return null;

  return (
    <div style={{
      position: 'fixed',
      top: 0,
      left: 0,
      right: 0,
      zIndex: 9999,
      background: '#fbbf24',
      color: '#78350f',
      padding: '8px 20px',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      gap: '16px',
      fontSize: '14px',
      fontWeight: 600,
      boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
    }}>
      <span>
        Viewing as: <strong>{impersonatedTenantName}</strong> — Read Only Mode
      </span>
      <button
        onClick={stopImpersonation}
        style={{
          background: '#78350f',
          color: '#fbbf24',
          border: 'none',
          borderRadius: '4px',
          padding: '4px 12px',
          fontSize: '13px',
          fontWeight: 600,
          cursor: 'pointer',
        }}
      >
        Exit Impersonation
      </button>
    </div>
  );
};
