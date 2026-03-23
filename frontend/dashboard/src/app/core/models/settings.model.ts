export interface AccountSettings {
  id: number;
  name: string;
  locale: string;
  domain: string;
  autoResolveDuration: number;
  features: Record<string, boolean>;
}

export interface UserProfile {
  id: number;
  name: string;
  email: string;
  avatarUrl: string | null;
  availability: 'online' | 'offline' | 'busy';
}

export interface NotificationPreferences {
  emailNotifications: boolean;
  pushNotifications: boolean;
  mentionNotifications: boolean;
  assignmentNotifications: boolean;
}
