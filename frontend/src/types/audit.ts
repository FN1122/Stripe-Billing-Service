export interface AuditLogEntry {
  id: string;
  tenantId: string | null;
  tenantName: string;
  userId: string;
  userEmail: string;
  action: string;
  entityType: string;
  entityId: string;
  details: string;
  ipAddress: string;
  createdAt: string;
}
