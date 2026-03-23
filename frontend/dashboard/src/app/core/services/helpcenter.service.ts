import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Article, Portal, Category } from '@core/models/helpcenter.model';

@Injectable({
  providedIn: 'root',
})
export class HelpCenterService {
  private readonly api = inject(ApiService);

  getArticles(): Observable<Article[]> {
    return this.api.get('/articles');
  }

  getArticle(id: number): Observable<Article> {
    return this.api.get(`/articles/${id}`);
  }

  createArticle(data: Partial<Article>): Observable<Article> {
    return this.api.post('/articles', data);
  }

  updateArticle(id: number, data: Partial<Article>): Observable<Article> {
    return this.api.put(`/articles/${id}`, data);
  }

  deleteArticle(id: number): Observable<void> {
    return this.api.delete(`/articles/${id}`);
  }

  getPortals(): Observable<Portal[]> {
    return this.api.get('/portals');
  }

  getCategories(portalId: number): Observable<Category[]> {
    return this.api.get(`/portals/${portalId}/categories`);
  }
}
