import React, { useState } from 'react';
import { Nav, Tab, Form, Button } from 'react-bootstrap';

export const SettingsPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('general');

  return (
    <>
      <div style={{ padding: '20px' }}>
        <h2 style={{ marginBottom: '30px' }}>Settings</h2>

        <Tab.Container activeKey={activeTab} onSelect={(k) => setActiveTab(k || 'general')}>
          <div style={{ display: 'flex', gap: '20px' }}>
            {/* Sidebar */}
            <div style={{ minWidth: '200px' }}>
              <Nav variant="pills" className="flex-column">
                <Nav.Item>
                  <Nav.Link eventKey="general">General</Nav.Link>
                </Nav.Item>
                <Nav.Item>
                  <Nav.Link eventKey="billing">Billing</Nav.Link>
                </Nav.Item>
                <Nav.Item>
                  <Nav.Link eventKey="notifications">Notifications</Nav.Link>
                </Nav.Item>
                <Nav.Item>
                  <Nav.Link eventKey="security">Security</Nav.Link>
                </Nav.Item>
              </Nav>
            </div>

            {/* Content */}
            <div style={{ flex: 1 }}>
              <Tab.Content>
                <Tab.Pane eventKey="general">
                  <div style={{
                    background: 'white',
                    padding: '20px',
                    borderRadius: '8px',
                    border: '1px solid #e2e8f0',
                  }}>
                    <h4 className="mb-4">General Settings</h4>
                    <Form>
                      <Form.Group className="mb-3">
                        <Form.Label>Company Name</Form.Label>
                        <Form.Control type="text" placeholder="Enter company name" />
                      </Form.Group>
                      <Form.Group className="mb-3">
                        <Form.Label>Email</Form.Label>
                        <Form.Control type="email" placeholder="Enter email" />
                      </Form.Group>
                      <Button variant="primary">Save Changes</Button>
                    </Form>
                  </div>
                </Tab.Pane>

                <Tab.Pane eventKey="billing">
                  <div style={{
                    background: 'white',
                    padding: '20px',
                    borderRadius: '8px',
                    border: '1px solid #e2e8f0',
                  }}>
                    <h4 className="mb-4">Billing Settings</h4>
                    <p>Manage your billing preferences and payment methods.</p>
                  </div>
                </Tab.Pane>

                <Tab.Pane eventKey="notifications">
                  <div style={{
                    background: 'white',
                    padding: '20px',
                    borderRadius: '8px',
                    border: '1px solid #e2e8f0',
                  }}>
                    <h4 className="mb-4">Notification Settings</h4>
                    <Form>
                      <Form.Check
                        type="checkbox"
                        label="Email notifications"
                        defaultChecked
                      />
                      <Form.Check
                        type="checkbox"
                        label="Payment alerts"
                        defaultChecked
                      />
                      <Button variant="primary" className="mt-3">Save Changes</Button>
                    </Form>
                  </div>
                </Tab.Pane>

                <Tab.Pane eventKey="security">
                  <div style={{
                    background: 'white',
                    padding: '20px',
                    borderRadius: '8px',
                    border: '1px solid #e2e8f0',
                  }}>
                    <h4 className="mb-4">Security Settings</h4>
                    <Button variant="outline-danger">Change Password</Button>
                  </div>
                </Tab.Pane>
              </Tab.Content>
            </div>
          </div>
        </Tab.Container>
      </div>
    </>
  );
};

export default SettingsPage;
