export enum ConversationStatus {
  Open = 'open',
  Pending = 'pending',
  Resolved = 'resolved',
  Snoozed = 'snoozed',
}

export enum ConversationPriority {
  None = 'none',
  Low = 'low',
  Medium = 'medium',
  High = 'high',
  Urgent = 'urgent',
}

export enum MessageType {
  Incoming = 'incoming',
  Outgoing = 'outgoing',
  Activity = 'activity',
  Template = 'template',
}

export interface Attachment {
  id: number;
  messageId: number;
  fileType: string;
  fileName: string;
  fileUrl: string;
  fileSize: number;
  thumbUrl?: string;
  createdAt: string;
}

export interface Message {
  id: number;
  conversationId: number;
  content: string;
  contentType: 'text' | 'html';
  messageType: MessageType;
  private: boolean;
  senderId?: number;
  senderType?: 'user' | 'contact' | 'agent_bot';
  senderName?: string;
  senderAvatar?: string;
  attachments: Attachment[];
  createdAt: string;
  updatedAt: string;
}

export interface Conversation {
  id: number;
  accountId: number;
  inboxId: number;
  contactId: number;
  assigneeId?: number;
  teamId?: number;
  status: ConversationStatus;
  priority: ConversationPriority;
  displayId: number;
  unreadCount: number;
  lastActivityAt: string;
  contactLastSeenAt?: string;
  agentLastSeenAt?: string;
  additionalAttributes: Record<string, unknown>;
  customAttributes: Record<string, unknown>;
  labels: string[];
  snoozedUntil?: string;
  messages: Message[];
  contact?: {
    id: number;
    name: string;
    email?: string;
    phone?: string;
    avatar?: string;
  };
  assignee?: {
    id: number;
    name: string;
    avatar?: string;
  };
  team?: {
    id: number;
    name: string;
  };
  inbox?: {
    id: number;
    name: string;
    channelType: string;
  };
  createdAt: string;
  updatedAt: string;
}
