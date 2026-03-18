import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { BehaviorSubject } from 'rxjs';

interface Article {
  id: number;
  title: string;
  status: 'draft' | 'published';
  category: string;
  author: string;
  views: number;
  createdAt: string;
  updatedAt: string;
}

@Component({
  selector: 'app-article-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-lg font-semibold text-gray-900">Help Center Articles</h2>
        <a
          routerLink="new"
          class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
        >
          New Article
        </a>
      </div>

      <!-- Loading State -->
      @if (loading$ | async) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if ((articles$ | async); as articles) {
          @if (articles.length === 0) {
            <div class="text-center py-12">
              <svg class="mx-auto h-12 w-12 text-gray-400" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M19.5 14.25v-2.625a3.375 3.375 0 0 0-3.375-3.375h-1.5A1.125 1.125 0 0 1 13.5 7.125v-1.5a3.375 3.375 0 0 0-3.375-3.375H8.25m0 12.75h7.5m-7.5 3H12M10.5 2.25H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 0 0-9-9Z" />
              </svg>
              <p class="mt-2 text-sm text-gray-500">No articles yet.</p>
              <a
                routerLink="new"
                class="mt-4 inline-block px-4 py-2 text-sm font-medium text-blue-600 hover:text-blue-700"
              >
                Write your first article
              </a>
            </div>
          } @else {
            <!-- Articles Table -->
            <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Title</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Category</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Author</th>
                    <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Views</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-gray-200">
                  @for (article of articles; track article.id) {
                    <tr class="hover:bg-gray-50 transition-colors">
                      <td class="px-6 py-4">
                        <a [routerLink]="[article.id]" class="text-sm font-medium text-gray-900 hover:text-blue-600">
                          {{ article.title }}
                        </a>
                      </td>
                      <td class="px-6 py-4">
                        <span
                          class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                          [class]="article.status === 'published' ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'"
                        >
                          {{ article.status === 'published' ? 'Published' : 'Draft' }}
                        </span>
                      </td>
                      <td class="px-6 py-4 text-sm text-gray-500">
                        {{ article.category }}
                      </td>
                      <td class="px-6 py-4 text-sm text-gray-500">
                        {{ article.author }}
                      </td>
                      <td class="px-6 py-4 text-sm text-gray-500 text-right">
                        {{ article.views | number }}
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }
        }
      }
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
    }
  `],
})
export class ArticleListComponent implements OnInit {
  private api = inject(ApiService);

  articles$ = new BehaviorSubject<Article[]>([]);
  loading$ = new BehaviorSubject<boolean>(false);

  ngOnInit(): void {
    this.loadArticles();
  }

  private loadArticles(): void {
    this.loading$.next(true);
    this.api.get<Article[]>('/articles').subscribe({
      next: (articles) => {
        this.articles$.next(articles);
        this.loading$.next(false);
      },
      error: () => {
        this.loading$.next(false);
      },
    });
  }
}
