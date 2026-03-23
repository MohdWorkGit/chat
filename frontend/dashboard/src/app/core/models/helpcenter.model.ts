export interface Article {
  id: number;
  title: string;
  content: string;
  description: string;
  slug: string;
  status: 'draft' | 'published' | 'archived';
  categoryId: number | null;
  portalId: number;
  locale: string;
  position: number;
  createdAt: string;
  updatedAt: string;
}

export interface Portal {
  id: number;
  name: string;
  slug: string;
  color: string;
  headerText: string;
  pageTitle: string;
  customDomain: string | null;
  createdAt: string;
}

export interface Category {
  id: number;
  name: string;
  slug: string;
  description: string;
  position: number;
  portalId: number;
  parentCategoryId: number | null;
}
