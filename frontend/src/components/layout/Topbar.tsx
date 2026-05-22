import React, { useState, useRef, useEffect, useCallback } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { dashboardApi } from '../../api/dashboardApi';
import { ActivityFeedItem } from '../../types/dashboard';
import { Bell, LogOut, Settings, CheckCircle, AlertTriangle, CreditCard, RefreshCw, Tag } from 'lucide-react';
import './Topbar.scss';

const getIconForType = (type: string, status: string) => {
  if (type === 'payment') return status === 'succeeded' ? CheckCircle : AlertTriangle;
  if (type === 'subscription') return CreditCard;
  if (type === 'refund') return RefreshCw;
  return Tag;
};

const getColorClass = (type: string, status: string) => {
  if (status === 'failed' || status === 'canceled') return 'alert';
  if (type === 'payment' && status === 'succeeded') return 'success';
  if (type === 'refund') return 'payment';
  if (type === 'subscription') return 'info';
  return 'info';
};

const timeAgo = (timestamp: string) => {
  const diff = Date.now() - new Date(timestamp).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 1) return 'just now';
  if (mins < 60) return `${mins}m ago`;
  const hours = Math.floor(mins / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 7) return `${days}d ago`;
  return new Date(timestamp).toLocaleDateString();
};

export const Topbar: React.FC = () => {
  const { user, logout, isImpersonating } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const isSuperAdmin = user?.role === 'SuperAdmin' && !isImpersonating;
  const [showNotifications, setShowNotifications] = useState(false);
  const [activities, setActivities] = useState<ActivityFeedItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [readIds, setReadIds] = useState<Set<string>>(() => {
    try {
      const saved = localStorage.getItem('readNotifications');
      return saved ? new Set(JSON.parse(saved)) : new Set();
    } catch { return new Set(); }
  });
  const dropdownRef = useRef<HTMLDivElement>(null);

  const fetchActivities = useCallback(async () => {
    try {
      setLoading(true);
      const { data } = await dashboardApi.getActivityFeed(15);
      if (data.isValid && data.data) {
        setActivities(data.data);
      }
    } catch (err) {
      console.error('Failed to fetch activity feed:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  // Fetch on mount and every 60 seconds (skip for SuperAdmin)
  useEffect(() => {
    if (isSuperAdmin) return;
    fetchActivities();
    const interval = setInterval(fetchActivities, 60000);
    return () => clearInterval(interval);
  }, [fetchActivities, isSuperAdmin]);

  // Persist read state
  useEffect(() => {
    localStorage.setItem('readNotifications', JSON.stringify([...readIds]));
  }, [readIds]);

  const unreadCount = activities.filter(a => !readIds.has(a.timestamp + a.type)).length;

  const getPageTitle = () => {
    const path = location.pathname.replace('/', '');
    if (!path) return 'Dashboard';
    return path.split('-').map(w => w.charAt(0).toUpperCase() + w.slice(1)).join(' ');
  };

  // Close dropdown on outside click
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target as Node))
        setShowNotifications(false);
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleLogout = async () => {
    await logout();
    window.location.href = '/login';
  };

  const markAllRead = () => {
    const all = new Set(readIds);
    activities.forEach(a => all.add(a.timestamp + a.type));
    setReadIds(all);
  };

  const markAsRead = (a: ActivityFeedItem) => {
    setReadIds(prev => new Set(prev).add(a.timestamp + a.type));
  };

  return (
    <header className="topbar">
      <div className="topbar-content">
        <div className="topbar-left">
          <h2 className="topbar-title">{getPageTitle()}</h2>
        </div>

        <div className="topbar-right">
          {/* Notifications - hidden for SuperAdmin */}
          {!isSuperAdmin && (
            <div className="notification-wrapper" ref={dropdownRef}>
              <button
                className={`topbar-icon-btn ${showNotifications ? 'active' : ''}`}
                title="Notifications"
                onClick={() => { setShowNotifications(!showNotifications); if (!showNotifications) fetchActivities(); }}
              >
                <Bell size={20} />
                {unreadCount > 0 && <span className="notification-badge">{unreadCount > 9 ? '9+' : unreadCount}</span>}
              </button>

              {showNotifications && (
                <div className="notification-dropdown">
                  <div className="notification-header">
                    <h4>Activity Feed</h4>
                    {unreadCount > 0 && (
                      <button className="mark-all-btn" onClick={markAllRead}>Mark all read</button>
                    )}
                  </div>
                  <div className="notification-list">
                    {loading && activities.length === 0 && (
                      <div className="notification-empty">Loading...</div>
                    )}
                    {!loading && activities.length === 0 && (
                      <div className="notification-empty">No recent activity</div>
                    )}
                    {activities.map((a, idx) => {
                      const Icon = getIconForType(a.type, a.status);
                      const colorClass = getColorClass(a.type, a.status);
                      const isRead = readIds.has(a.timestamp + a.type);
                      return (
                        <div
                          key={idx}
                          className={`notification-item ${!isRead ? 'unread' : ''}`}
                          onClick={() => markAsRead(a)}
                        >
                          <div className={`notification-icon ${colorClass}`}>
                            <Icon size={16} />
                          </div>
                          <div className="notification-body">
                            <p className="notification-title">{a.title}</p>
                            <p className="notification-message">{a.description}</p>
                            <span className="notification-time">{timeAgo(a.timestamp)}</span>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                  <div className="notification-footer">
                    <button onClick={() => { setShowNotifications(false); navigate('/audit-logs'); }}>
                      View all activity
                    </button>
                  </div>
                </div>
              )}
            </div>
          )}

          {/* Settings */}
          <button
            className={`topbar-icon-btn ${location.pathname === '/settings' ? 'active' : ''}`}
            title="Settings"
            onClick={() => navigate('/settings')}
          >
            <Settings size={20} />
          </button>

          {/* Logout */}
          <button className="topbar-icon-btn logout-btn" title="Logout" onClick={handleLogout}>
            <LogOut size={20} />
          </button>

          <div className="topbar-user">
            <div className="user-avatar">
              {user?.fullName?.charAt(0).toUpperCase()}
            </div>
            <div className="user-info">
              <p className="user-name">{user?.fullName}</p>
              <p className="user-role">{user?.role}</p>
            </div>
          </div>
        </div>
      </div>
    </header>
  );
};
