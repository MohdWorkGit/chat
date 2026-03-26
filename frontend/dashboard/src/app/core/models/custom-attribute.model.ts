export interface CustomAttribute {
  id: number;
  accountId: number;
  displayName: string;
  key: string;
  attributeType: 'text' | 'number' | 'date' | 'list' | 'checkbox' | 'link' | 'currency';
  appliedTo: 'contact' | 'conversation';
  description?: string;
  listValues?: string[];
  createdAt: string;
  updatedAt: string;
}
