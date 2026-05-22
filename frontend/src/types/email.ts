export interface EmailTemplate {
  id: string;
  templateKey: string;
  subject: string;
  htmlBody: string;
  plainTextBody?: string;
  isActive: boolean;
  variables: string[];
  createdAt: string;
  updatedAt?: string;
}

export interface EmailLog {
  id: string;
  templateKey?: string;
  to: string;
  subject: string;
  status: string;
  provider?: string;
  errorMessage?: string;
  sentAt?: string;
  deliveredAt?: string;
  createdAt: string;
}

export type EmailTemplateResponse = EmailTemplate;
export type EmailLogResponse = EmailLog;

export interface EmailStats {
  totalSent: number;
  totalDelivered: number;
  totalFailed: number;
  totalBounced: number;
  deliveryRate: number;
}
