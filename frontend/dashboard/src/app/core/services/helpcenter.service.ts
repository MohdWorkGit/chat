import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ApiService } from './api.service';
import { Article, Portal, Category } from '@core/models/helpcenter.model';

@Injectable({
  providedIn: 'root',
})
export class HelpCenterService {
  private readonly api = inject(ApiService);

  getPortals(): Observable<Portal[]> {
    return this.api.get<{ data: Portal[] } | Portal[]>('/portals').pipe(
      map((res) => (Array.isArray(res) ? res : res?.data ?? []))
    );
  }

  getPortal(id: number): Observable<Portal> {
    return this.api.get(`/portals/${id}`);
  }

  createPortal(data: Partial<Portal>): Observable<{ id: number }> {
    return this.api.post(`/portals`, data);
  }

  updatePortal(id: number, data: Partial<Portal>): Observable<void> {
    return this.api.put(`/portals/${id}`, data);
  }

  deletePortal(id: number): Observable<void> {
    return this.api.delete(`/portals/${id}`);
  }

  uploadPortalLogo(id: number, file: File): Observable<{ id: number; logoUrl: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.api.upload(`/portals/${id}/logo`, formData);
  }

  getArticles(portalId: number): Observable<Article[]> {
    return this.api
      .get<{ items: Article[] } | { data: Article[] } | Article[]>(`/portals/${portalId}/articles`)
      .pipe(
        map((res) => {
          if (Array.isArray(res)) return res;
          if (res && 'items' in res) return res.items ?? [];
          if (res && 'data' in res) return res.data ?? [];
          return [];
        })
      );
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

  getCategory(portalId: number, id: number): Observable<Category> {
    return this.api.get(`/portals/${portalId}/categories/${id}`);
  }

  createCategory(portalId: number, data: Partial<Category>): Observable<Category> {
    return this.api.post(`/portals/${portalId}/categories`, data);
  }

  updateCategory(portalId: number, id: number, data: Partial<Category>): Observable<Category> {
    return this.api.put(`/portals/${portalId}/categories/${id}`, data);
  }

  deleteCategory(portalId: number, id: number): Observable<void> {
    return this.api.delete(`/portals/${portalId}/categories/${id}`);
  }
}
