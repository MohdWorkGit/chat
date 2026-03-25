export enum UserRole {
  Administrator = 'administrator',
  Agent = 'agent',
  Supervisor = 'supervisor',
}

export enum UserAvailability {
  Online = 'online',
  Offline = 'offline',
  Busy = 'busy',
}

export interface User {
  id: number;
  name: string;
  displayName?: string;
  email: string;
  avatar?: string;
  role?: string;
  availability?: string;
  availabilityStatus?: string;
  accountId: number;
  createdAt: string;
}

export interface AccountUser {
  id: number;
  userId: number;
  accountId: number;
  role: UserRole;
  availability: UserAvailability;
  autoOffline: boolean;
}
