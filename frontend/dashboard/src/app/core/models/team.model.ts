export interface TeamMember {
  id: number;
  teamId: number;
  userId: number;
  userName?: string;
  userAvatar?: string;
}

export interface Team {
  id: number;
  accountId: number;
  name: string;
  description?: string;
  allowAutoAssign: boolean;
  members: TeamMember[];
  createdAt: string;
  updatedAt: string;
}
