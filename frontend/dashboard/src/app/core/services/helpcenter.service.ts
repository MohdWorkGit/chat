import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Article, Portal, Category } from '@core/models/helpcenter.model';

@Injectable({
  providedIn: 'root',
})
export class HelpCenterService {
  private readonly api = inject(ApiService);

  getPortals(): Observable<Portal[]> {
    return this.api.get('/portals');
  }

  getArticles(portalId: number): Observable<Article[]> {
    return this.api.get(`/portals/${portalId}/articles`);
  }

  getArticle(portalId: number, id: number): Observable<Article> {
    return this.api.get(`/portals/${portalId}/articles/${id}`);
  }

  createArticle(portalId: number, data: Partial<Article>): Observable<Article> {
    return this.api.post(`/portals/${portalId}/articles`, data);
  }

  updateArticle(portalId: number, id: number, data: Partial<Article>): Observable<Article> {
    return this.api.put(`/portals/${portalId}/articles/${id}`, data);
  }

  deleteArticle(portalId: number, id: number): Observable<void> {
    return this.api.delete(`/portals/${portalId}/articles/${id}`);
  }

  getCategories(portalId: number): Observable<Category[]> {
    return this.api.get(`/portals/${portalId}/categories`);
  }
}
