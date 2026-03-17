export interface WorkingHour {
  dayOfWeek: number;
  closedAllDay: boolean;
  openHour?: number;
  openMinutes?: number;
  closeHour?: number;
  closeMinutes?: number;
  openAllDay: boolean;
}

export interface InboxMember {
  id: number;
  userId: number;
  inboxId: number;
  userName?: string;
  userAvatar?: string;
}

export interface Inbox {
  id: number;
  accountId: number;
  name: string;
  channelType: string;
  channelId: number;
  avatarUrl?: string;
  greeting: boolean;
  greetingMessage?: string;
  enableAutoAssignment: boolean;
  enableEmailCollect: boolean;
  workingHours: WorkingHour[];
  outOfOfficeMessage?: string;
  timezone?: string;
  csatSurveyEnabled: boolean;
  allowMessagesAfterResolved: boolean;
  members: InboxMember[];
  createdAt: string;
  updatedAt: string;
}
