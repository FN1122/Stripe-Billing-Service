import React from 'react';
import { BrowserRouter as Router } from 'react-router-dom';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { AuthProvider } from './contexts/AuthContext';
import { ToastProvider } from './contexts/ToastContext';
import { SidebarProvider } from './contexts/SidebarContext';
import { setupInterceptors } from './api/interceptors';
import { AppRoutes } from './routes/AppRoutes';
import './App.scss';

// Setup API interceptors
setupInterceptors();

function App() {
  return (
    <Router>
      <AuthProvider>
        <ToastProvider>
          <SidebarProvider>
            <AppRoutes />
            <ToastContainer />
          </SidebarProvider>
        </ToastProvider>
      </AuthProvider>
    </Router>
  );
}

export default App;
