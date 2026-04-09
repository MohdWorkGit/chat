import { Component, ChangeDetectionStrategy, OnInit, Input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { PortalApiService, Category, ArticleSummary } from '../../services/portal-api.service';
import { BreadcrumbComponent, BreadcrumbItem } from '../../components/breadcrumb/breadcrumb.component';

@Component({
  selector: 'portal-category',
  standalone: true,
  imports: [CommonModule, RouterLink, BreadcrumbComponent],
  template: `
    <div class="container" style="padding-top: 16px;">
      @if (category(); as category) {
        <portal-breadcrumb [items]="breadcrumbItems()" />

        <div style="max-width: 800px; margin: 0 auto; padding: 32px 0;">
          <h1 style="font-size: 2rem; font-weight: 700; margin-bottom: 8px;">{{ category.name }}</h1>
          @if (category.description) {
            <p style="color: var(--portal-text-secondary); margin-bottom: 32px;">{{ category.description }}</p>
          }

          <ul class="article-list">
            @for (article of articles(); track article.id) {
              <li class="article-list-item">
                <a [routerLink]="['/article', article.slug]">{{ article.title }}</a>
                <p>{{ article.description }}</p>
              </li>
            } @empty {
              <li style="padding: 32px 0; text-align: center; color: var(--portal-text-secondary);">
                No articles found in this category.
              </li>
            }
          </ul>
        </div>
      } @else {
        <div style="text-align: center; padding: 64px 0;">
          <p style="color: var(--portal-text-secondary);">Loading category...</p>
        </div>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoryComponent implements OnInit {
  @Input() slug = '';

  category = signal<Category | null>(null);
  articles = signal<ArticleSummary[]>([]);
  breadcrumbItems = signal<BreadcrumbItem[]>([]);

  constructor(
    private readonly apiService: PortalApiService,
    private readonly titleService: Title,
  ) {}

  ngOnInit(): void {
    if (this.slug) {
      this.loadCategory(this.slug);
    }
  }

  private loadCategory(slug: string): void {
    this.apiService.getCategory(slug).subscribe(category => {
      this.category.set(category);
      this.titleService.setTitle(`${category.name} - Help Center`);
      this.breadcrumbItems.set([
        { label: 'Home', url: '/' },
        { label: category.name },
      ]);
    });

    this.apiService.getCategoryArticles(slug).subscribe(articles => {
      this.articles.set(articles);
    });
  }
}
