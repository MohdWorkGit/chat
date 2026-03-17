import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

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

@Injectable({ providedIn: 'root' })
export class PortalApiService {
  private readonly baseUrl = '/api/v1/portal';

  constructor(private readonly http: HttpClient) {}

  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${this.baseUrl}/categories`);
  }

  getCategory(slug: string): Observable<Category> {
    return this.http.get<Category>(`${this.baseUrl}/categories/${slug}`);
  }

  getCategoryArticles(slug: string): Observable<ArticleSummary[]> {
    return this.http.get<ArticleSummary[]>(`${this.baseUrl}/categories/${slug}/articles`);
  }

  getArticle(slug: string): Observable<Article> {
    return this.http.get<Article>(`${this.baseUrl}/articles/${slug}`);
  }

  searchArticles(query: string): Observable<ArticleSummary[]> {
    const params = new HttpParams().set('q', query);
    return this.http.get<ArticleSummary[]>(`${this.baseUrl}/articles/search`, { params });
  }
}
