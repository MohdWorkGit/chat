import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';

export interface Category {
  id: number;
  name: string;
  slug: string;
  description?: string;
  position: number;
  locale?: string;
  parentCategoryId?: number | null;
  articleCount: number;
}

export interface ArticleSummary {
  id: number;
  title: string;
  slug: string;
  description?: string;
  categoryId?: number | null;
  updatedAt: string;
}

export interface Article {
  id: number;
  title: string;
  slug: string;
  description?: string;
  content: string;
  updatedAt: string;
  category?: {
    id: number;
    name: string;
    slug: string;
  };
}

export interface TocEntry {
  level: number;
  id: string;
  text: string;
}

export interface PageMeta {
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

interface PaginatedEnvelope<T> {
  data: T[];
  meta?: PageMeta;
}

export interface PagedResult<T> {
  items: T[];
  meta: PageMeta;
}

@Injectable({ providedIn: 'root' })
export class PortalApiService {
  // Backend route is /api/v1/public/portals/{portalSlug}/... so requests must
  // be scoped to a portal slug. Configurable via PORTAL_SLUG env var (read at
  // SSR build/runtime) or the `portal-slug` localStorage key on the client.
  private readonly portalSlug = this.resolvePortalSlug();
  private readonly baseUrl = `/api/v1/public/portals/${this.portalSlug}`;

  constructor(private readonly http: HttpClient) {}

  private resolvePortalSlug(): string {
    if (typeof process !== 'undefined' && process.env && process.env['PORTAL_SLUG']) {
      return process.env['PORTAL_SLUG'] as string;
    }
    if (typeof localStorage !== 'undefined') {
      return localStorage.getItem('portal-slug') || 'default';
    }
    return 'default';
  }

  private getLocale(): string {
    if (typeof localStorage !== 'undefined') {
      return localStorage.getItem('portal-locale') || 'en';
    }
    return 'en';
  }

  private applyLocale(params: HttpParams): HttpParams {
    return params.set('locale', this.getLocale());
  }

  // Backend responses are wrapped in { Data: [...], Meta?: {...} } and
  // ASP.NET Core serializes with camelCase by default. Unwrap to plain arrays.
  private unwrap<T>(envelope: PaginatedEnvelope<T> | T[] | null | undefined): T[] {
    if (!envelope) return [];
    if (Array.isArray(envelope)) return envelope;
    return envelope.data ?? [];
  }

  getCategories(): Observable<Category[]> {
    const params = this.applyLocale(new HttpParams());
    return this.http
      .get<PaginatedEnvelope<Category>>(`${this.baseUrl}/categories`, { params })
      .pipe(map((response) => this.unwrap<Category>(response)));
  }

  // Backend has no get-by-slug endpoint for categories — fetch the list and
  // pick the matching slug client-side.
  getCategory(slug: string): Observable<Category> {
    return this.getCategories().pipe(
      map((categories) => {
        const found = categories.find((c) => c.slug === slug);
        if (!found) {
          throw new Error(`Category not found: ${slug}`);
        }
        return found;
      }),
    );
  }

  // Backend filters articles by category id (?categoryId=N), not by slug,
  // so we resolve the slug to an id first.
  getCategoryArticles(slug: string): Observable<ArticleSummary[]> {
    return this.getCategory(slug).pipe(
      switchMap((category) => {
        const params = this.applyLocale(new HttpParams()).set('categoryId', String(category.id));
        return this.http
          .get<PaginatedEnvelope<ArticleSummary>>(`${this.baseUrl}/articles`, { params })
          .pipe(map((response) => this.unwrap<ArticleSummary>(response)));
      }),
    );
  }

  getArticle(slug: string): Observable<Article> {
    const params = this.applyLocale(new HttpParams());
    return this.http.get<Article>(`${this.baseUrl}/articles/${slug}`, { params });
  }

  searchArticles(query: string, page: number = 1, pageSize: number = 10): Observable<PagedResult<ArticleSummary>> {
    const params = this.applyLocale(new HttpParams())
      .set('q', query.trim())
      .set('page', String(page))
      .set('pageSize', String(pageSize));
    return this.http
      .get<PaginatedEnvelope<ArticleSummary>>(`${this.baseUrl}/articles/search`, { params })
      .pipe(
        map((response) => ({
          items: this.unwrap<ArticleSummary>(response),
          meta: response?.meta ?? { totalCount: 0, page, pageSize, totalPages: 0 },
        })),
      );
  }
}
