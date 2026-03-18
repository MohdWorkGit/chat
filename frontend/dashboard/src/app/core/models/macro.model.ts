export interface Macro {
  id: number;
  accountId: number;
  name: string;
  actions?: string;
  visibility: 'personal' | 'global';
  createdById?: number;
  createdAt: string;
  updatedAt: string;
}
