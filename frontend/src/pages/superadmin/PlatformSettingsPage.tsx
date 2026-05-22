import React, { useEffect, useState } from 'react';
import { Form, Button, Card, Alert } from 'react-bootstrap';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { superAdminApi, PlatformSettings } from '../../api/superAdminApi';

export const PlatformSettingsPage: React.FC = () => {
  const [settings, setSettings] = useState<PlatformSettings>({
    platformName: '',
    defaultCurrency: 'usd',
    maintenanceMode: false,
    defaultFeatures: '',
    maxTenantsAllowed: 100,
  });
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState<{ type: string; text: string } | null>(null);

  useEffect(() => {
    const fetchSettings = async () => {
      try {
        const res = await superAdminApi.getPlatformSettings();
        if (res.data.isValid && res.data.data) {
          setSettings(res.data.data);
        }
      } catch (err) {
        console.error('Failed to fetch settings:', err);
      } finally {
        setIsLoading(false);
      }
    };
    fetchSettings();
  }, []);

  const handleSave = async () => {
    setIsSaving(true);
    setMessage(null);
    try {
      const res = await superAdminApi.updatePlatformSettings(settings);
      if (res.data.isValid) {
        setMessage({ type: 'success', text: 'Settings saved successfully' });
      } else {
        setMessage({ type: 'danger', text: res.data.message || 'Failed to save settings' });
      }
    } catch (err) {
      setMessage({ type: 'danger', text: 'Failed to save settings' });
    } finally {
      setIsSaving(false);
    }
  };

  if (isLoading) {
    return (
      <div style={{ padding: '20px' }}>
        <h2 style={{ marginBottom: '20px' }}>Platform Settings</h2>
        <LoadingSkeleton count={4} height={50} />
      </div>
    );
  }

  return (
    <div style={{ padding: '20px' }}>
      <h2 style={{ marginBottom: '20px' }}>Platform Settings</h2>

      {message && <Alert variant={message.type} onClose={() => setMessage(null)} dismissible>{message.text}</Alert>}

      <Card style={{ border: '1px solid #e2e8f0', borderRadius: '8px' }}>
        <Card.Body>
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Platform Name</Form.Label>
              <Form.Control
                type="text"
                value={settings.platformName || ''}
                onChange={(e) => setSettings({ ...settings, platformName: e.target.value })}
              />
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Label>Default Currency</Form.Label>
              <Form.Select
                value={settings.defaultCurrency || 'usd'}
                onChange={(e) => setSettings({ ...settings, defaultCurrency: e.target.value })}
              >
                <option value="usd">USD</option>
                <option value="eur">EUR</option>
                <option value="gbp">GBP</option>
                <option value="cad">CAD</option>
                <option value="aud">AUD</option>
              </Form.Select>
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Label>Max Tenants Allowed</Form.Label>
              <Form.Control
                type="number"
                value={settings.maxTenantsAllowed}
                onChange={(e) => setSettings({ ...settings, maxTenantsAllowed: parseInt(e.target.value) || 0 })}
              />
            </Form.Group>

            <Form.Group className="mb-3">
              <Form.Label>Default Features (JSON)</Form.Label>
              <Form.Control
                as="textarea"
                rows={3}
                value={settings.defaultFeatures || ''}
                onChange={(e) => setSettings({ ...settings, defaultFeatures: e.target.value })}
                placeholder='["feature1", "feature2"]'
              />
            </Form.Group>

            <Form.Group className="mb-4">
              <Form.Check
                type="switch"
                label="Maintenance Mode"
                checked={settings.maintenanceMode}
                onChange={(e) => setSettings({ ...settings, maintenanceMode: e.target.checked })}
              />
              <Form.Text className="text-muted">
                When enabled, tenant users will see a maintenance message.
              </Form.Text>
            </Form.Group>

            <Button variant="primary" onClick={handleSave} disabled={isSaving}>
              {isSaving ? 'Saving...' : 'Save Settings'}
            </Button>
          </Form>
        </Card.Body>
      </Card>
    </div>
  );
};

export default PlatformSettingsPage;
