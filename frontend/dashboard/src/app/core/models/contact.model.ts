export enum ContactType {
  Lead = 'lead',
  Customer = 'customer',
}

export interface Contact {
  id: number;
  accountId: number;
  name: string;
  email?: string;
  phone?: string;
  avatar?: string;
  contactType: ContactType;
  identifier?: string;
  company?: {
    id: number;
    name: string;
  };
  additionalAttributes: Record<string, unknown>;
  customAttributes: Record<string, unknown>;
  lastActivityAt?: string;
  location?: string;
  conversationsCount: number;
  createdAt: string;
  updatedAt: string;
}
