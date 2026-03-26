import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

export interface Category {
  id: number;
  name: string;
  slug: string;
  description: string;
  icon: string;
  articleCount: number;
}

export interface ArticleSummary {
  id: number;
  title: string;
  slug: string;
  description: string;
  categoryName?: string;
  updatedAt: string;
}

export interface Article {
  id: number;
  title: string;
  slug: string;
  description: string;
  contentHtml: string;
  updatedAt: string;
  category: {
    name: string;
    slug: string;
  };
}

export interface TocEntry {
  level: number;
  id: string;
  text: string;
}

export interface PaginatedResult<T> {
  items: T[];
  total: number;
  page: number;
  perPage: number;
}

@Injectable({ providedIn: 'root' })
export class PortalApiService {
  private readonly baseUrl = '/api/v1/portal';

  constructor(private readonly http: HttpClient) {}

  private getLocale(): string {
    if (typeof localStorage !== 'undefined') {
      return localStorage.getItem('portal-locale') || 'en';
    }
    return 'en';
  }

  private applyLocale(params: HttpParams): HttpParams {
    return params.set('locale', this.getLocale());
  }

  getCategories(): Observable<Category[]> {
    const params = this.applyLocale(new HttpParams());
    return this.http.get<Category[]>(`${this.baseUrl}/categories`, { params }).pipe(
      catchError(() => of([])),
    );
  }

  getCategory(slug: string): Observable<Category> {
    const params = this.applyLocale(new HttpParams());
    return this.http.get<Category>(`${this.baseUrl}/categories/${slug}`, { params });
  }

  getCategoryArticles(slug: string): Observable<ArticleSummary[]> {
    const params = this.applyLocale(new HttpParams());
    return this.http.get<ArticleSummary[]>(`${this.baseUrl}/categories/${slug}/articles`, { params }).pipe(
      catchError(() => of([])),
    );
  }

  getArticle(slug: string): Observable<Article> {
    const params = this.applyLocale(new HttpParams());
    return this.http.get<Article>(`${this.baseUrl}/articles/${slug}`, { params });
  }

  searchArticles(query: string, page: number = 1, perPage: number = 10): Observable<ArticleSummary[]> {
    let params = new HttpParams()
      .set('q', query)
      .set('page', page.toString())
      .set('perPage', perPage.toString());
    params = this.applyLocale(params);
    return this.http.get<ArticleSummary[]>(`${this.baseUrl}/articles/search`, { params }).pipe(
      catchError(() => of([])),
    );
  }
}
