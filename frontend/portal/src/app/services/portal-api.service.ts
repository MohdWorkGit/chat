import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';

export interface Category {
  id: number;
  name: string;
  slug: string;
  description?: string;
  position: number;
  locale?: string;
  parentCategoryId?: number | null;
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

interface PaginatedEnvelope<T> {
  data: T[];
  meta?: {
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
  };
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
      .pipe(
        map((response) => this.unwrap<Category>(response)),
        catchError(() => of<Category[]>([])),
      );
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
      catchError(() => of<ArticleSummary[]>([])),
    );
  }

  getArticle(slug: string): Observable<Article> {
    const params = this.applyLocale(new HttpParams());
    return this.http.get<Article>(`${this.baseUrl}/articles/${slug}`, { params });
  }

  // TODO: backend has no /articles/search endpoint yet. As a stopgap we fetch
  // the article list and filter client-side. Replace with a proper backend
  // search endpoint when available.
  searchArticles(query: string, _page: number = 1, _perPage: number = 10): Observable<ArticleSummary[]> {
    const params = this.applyLocale(new HttpParams());
    return this.http
      .get<PaginatedEnvelope<ArticleSummary>>(`${this.baseUrl}/articles`, { params })
      .pipe(
        map((response) => this.unwrap<ArticleSummary>(response)),
        map((articles) => {
          const q = query.trim().toLowerCase();
          if (!q) return articles;
          return articles.filter((a) =>
            a.title.toLowerCase().includes(q) ||
            (a.description ?? '').toLowerCase().includes(q),
          );
        }),
        catchError(() => of<ArticleSummary[]>([])),
      );
  }
}
