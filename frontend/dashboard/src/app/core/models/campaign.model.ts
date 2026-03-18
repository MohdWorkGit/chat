export interface Campaign {
  id: number;
  accountId: number;
  inboxId: number;
  title: string;
  description?: string;
  message?: string;
  campaignType: 'ongoing' | 'one_off';
  status: 'draft' | 'active' | 'completed';
  audience?: string;
  scheduledAt?: string;
  enabled: boolean;
  createdAt: string;
  updatedAt: string;
}
