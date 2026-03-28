export interface Webhook {
  id: number;
  accountId: number;
  inboxId?: number;
  url: string;
  subscriptions: string[];
  hmacToken?: string;
  hmacMandatory: boolean;
  createdAt: string;
  updatedAt: string;
}

export const WEBHOOK_EVENTS = [
  'conversation_created',
  'conversation_updated',
  'conversation_status_changed',
  'message_created',
  'message_updated',
  'contact_created',
  'contact_updated',
  'webwidget_triggered',
  'inbox_created',
  'inbox_updated',
] as const;
