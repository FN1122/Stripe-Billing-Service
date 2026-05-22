import React, { createContext, useState, useCallback } from 'react';

export interface SidebarContextType {
  isCollapsed: boolean;
  toggle: () => void;
}

export const SidebarContext = createContext<SidebarContextType | undefined>(undefined);

export const SidebarProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [isCollapsed, setIsCollapsed] = useState(false);

  const toggle = useCallback(() => {
    setIsCollapsed((prev) => !prev);
  }, []);

  const value: SidebarContextType = {
    isCollapsed,
    toggle,
  };

  return <SidebarContext.Provider value={value}>{children}</SidebarContext.Provider>;
};
