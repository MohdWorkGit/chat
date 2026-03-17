export interface Notification {
  id: number;
  accountId: number;
  userId: number;
  notificationType: 'conversation_creation' | 'conversation_assignment' | 'assigned_conversation_new_message' | 'participating_conversation_new_message' | 'mention';
  primaryActorId: number;
  primaryActorType: string;
  secondaryActorId?: number;
  secondaryActorType?: string;
  readAt?: string;
  snoozedUntil?: string;
  meta: Record<string, unknown>;
  createdAt: string;
  updatedAt: string;
}

export interface NotificationSetting {
  id: number;
  userId: number;
  accountId: number;
  emailConversationCreation: boolean;
  emailConversationAssignment: boolean;
  emailNewMessage: boolean;
  emailMention: boolean;
  pushConversationCreation: boolean;
  pushConversationAssignment: boolean;
  pushNewMessage: boolean;
  pushMention: boolean;
}
