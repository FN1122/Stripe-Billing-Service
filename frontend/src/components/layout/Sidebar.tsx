import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import {
  BarChart3,
  CreditCard,
  DollarSign,
  Menu,
  Settings,
  Users,
  FileText,
  Key,
  Webhook,
  Eye,
  Building2,
  History,
  Tag,
  Activity,
  Receipt,
  AlertTriangle,
  Wallet,
  Mail,
  Download,
  Link2,
  Radio,
  LayoutDashboard,
  PieChart,
  Shield,
} from 'lucide-react';
import { useSidebar } from '../../hooks/useSidebar';
import { useAuth } from '../../hooks/useAuth';
import './Sidebar.scss';

export const Sidebar: React.FC = () => {
  const { isCollapsed, toggle } = useSidebar();
  const { user, isImpersonating } = useAuth();
  const location = useLocation();

  const isAdmin = user?.role === 'Admin';
  const isSuperAdmin = user?.role === 'SuperAdmin' && !isImpersonating;

  // SuperAdmin (not impersonating) gets a completely different sidebar
  const superAdminMenuItems = [
    {
      category: 'PLATFORM ADMIN',
      items: [
        { icon: LayoutDashboard, label: 'Dashboard', path: '/super-admin/dashboard' },
        { icon: Building2, label: 'Tenants', path: '/super-admin/tenants' },
        { icon: Mail, label: 'Email Templates', path: '/super-admin/email-templates' },
        { icon: Settings, label: 'Platform Settings', path: '/super-admin/settings' },
        { icon: Shield, label: 'Audit Log', path: '/super-admin/audit' },
      ],
    },
  ];

  // Regular tenant-scoped menu items (for Admin, Manager, Viewer, and impersonating SuperAdmin)
  const tenantMenuItems = [
    {
      category: 'BILLING',
      items: [
        { icon: DollarSign, label: 'Payments', path: '/payments' },
        { icon: CreditCard, label: 'Subscriptions', path: '/subscriptions' },
        { icon: Users, label: 'Customers', path: '/customers' },
        { icon: FileText, label: 'Invoices', path: '/invoices' },
        { icon: Eye, label: 'Refunds', path: '/refunds' },
        { icon: Tag, label: 'Coupons', path: '/coupons' },
        { icon: Activity, label: 'Usage Billing', path: '/usage-billing' },
        { icon: Wallet, label: 'Credits', path: '/credits' },
        { icon: AlertTriangle, label: 'Dunning', path: '/dunning' },
      ],
    },
    {
      category: 'ANALYTICS',
      items: [{ icon: BarChart3, label: 'Analytics', path: '/analytics' }],
    },
    {
      category: 'GATEWAY',
      items: [
        { icon: Key, label: 'API Keys', path: '/api-keys' },
        { icon: History, label: 'Logs', path: '/logs' },
        { icon: Webhook, label: 'Webhooks', path: '/webhooks' },
        { icon: Radio, label: 'Webhook Events', path: '/webhook-events' },
      ],
    },
    ...(isAdmin
      ? [
          {
            category: 'MANAGEMENT',
            items: [
              { icon: Users, label: 'Users', path: '/users' },
              { icon: History, label: 'Audit Logs', path: '/audit-logs' },
              { icon: Settings, label: 'Settings', path: '/settings' },
              { icon: Receipt, label: 'Tax Config', path: '/tax-config' },
              { icon: Mail, label: 'Email Templates', path: '/email-templates' },
              { icon: Download, label: 'Exports', path: '/exports' },
              { icon: Link2, label: 'Stripe Connect', path: '/connect' },
            ],
          },
        ]
      : []),
  ];

  const menuItems = isSuperAdmin ? superAdminMenuItems : tenantMenuItems;

  return (
    <aside className={`sidebar ${isCollapsed ? 'collapsed' : ''}`}>
      <div className="sidebar-header">
        <div className="sidebar-logo">
          {!isCollapsed && <span className="logo-text">{isSuperAdmin ? 'Platform' : 'Billing'}</span>}
        </div>
        <button className="sidebar-toggle btn btn-sm" onClick={toggle}>
          <Menu size={20} />
        </button>
      </div>

      <nav className="sidebar-nav">
        {menuItems.map((group, idx) => (
          <div key={idx} className="nav-section">
            {!isCollapsed && <div className="nav-section-title">{group.category}</div>}
            <ul className="nav-items">
              {group.items.map((item) => {
                const Icon = item.icon;
                return (
                  <li key={item.path}>
                    <Link to={item.path} className={`nav-link ${location.pathname === item.path || location.pathname.startsWith(item.path + '/') ? 'active' : ''}`} title={isCollapsed ? item.label : ''}>
                      <Icon size={20} />
                      {!isCollapsed && <span>{item.label}</span>}
                    </Link>
                  </li>
                );
              })}
            </ul>
          </div>
        ))}
      </nav>
    </aside>
  );
};
