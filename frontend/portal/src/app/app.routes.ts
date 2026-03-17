import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent),
    title: 'Help Center',
  },
  {
    path: 'article/:slug',
    loadComponent: () => import('./pages/article/article.component').then(m => m.ArticleComponent),
  },
  {
    path: 'category/:slug',
    loadComponent: () => import('./pages/category/category.component').then(m => m.CategoryComponent),
  },
  {
    path: 'search',
    loadComponent: () => import('./pages/search/search.component').then(m => m.SearchComponent),
    title: 'Search - Help Center',
  },
  {
    path: '**',
    redirectTo: '',
  },
];
