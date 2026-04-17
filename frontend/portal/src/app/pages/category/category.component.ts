import { Component, ChangeDetectionStrategy, OnInit, Input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { PortalApiService, Category, ArticleSummary, RelatedCategorySummary } from '../../services/portal-api.service';
import { BreadcrumbComponent, BreadcrumbItem } from '../../components/breadcrumb/breadcrumb.component';

@Component({
  selector: 'portal-category',
  standalone: true,
  imports: [CommonModule, RouterLink, BreadcrumbComponent],
  template: `
    <div class="container" style="padding-top: 16px;">
      @if (loading()) {
        <div style="text-align: center; padding: 64px 0;">
          <p style="color: var(--portal-text-secondary);">Loading category…</p>
        </div>
      } @else if (error()) {
        <div style="text-align: center; padding: 64px 0;">
          <p style="color: #b91c1c;">{{ error() }}</p>
          <button class="portal-pagination-btn" style="margin-top: 12px;" (click)="reload()">Try again</button>
        </div>
      } @else {
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
                  @if (article.description) {
                    <p>{{ article.description }}</p>
                  }
                </li>
              } @empty {
                <li style="padding: 32px 0; text-align: center; color: var(--portal-text-secondary);">
                  No articles found in this category.
                </li>
              }
            </ul>

            @if (relatedCategories().length > 0) {
              <section class="portal-related-categories" style="margin-top: 48px;">
                <h2 style="font-size: 1.25rem; font-weight: 600; margin-bottom: 16px;">Related categories</h2>
                <ul class="related-category-list" style="list-style: none; padding: 0; margin: 0; display: grid; gap: 12px; grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));">
                  @for (related of relatedCategories(); track related.id) {
                    <li>
                      <a
                        [routerLink]="['/category', related.slug]"
                        style="display: block; padding: 16px; border: 1px solid var(--portal-border, #e5e7eb); border-radius: 8px; text-decoration: none; color: inherit;">
                        <strong style="display: block; margin-bottom: 4px;">{{ related.name }}</strong>
                        @if (related.description) {
                          <span style="color: var(--portal-text-secondary, #6b7280); font-size: 0.875rem;">{{ related.description }}</span>
                        }
                      </a>
                    </li>
                  }
                </ul>
              </section>
            }
          </div>
        }
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoryComponent implements OnInit {
  @Input() slug = '';

  category = signal<Category | null>(null);
  articles = signal<ArticleSummary[]>([]);
  relatedCategories = signal<RelatedCategorySummary[]>([]);
  breadcrumbItems = signal<BreadcrumbItem[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  constructor(
    private readonly apiService: PortalApiService,
    private readonly titleService: Title,
  ) {}

  ngOnInit(): void {
    if (this.slug) {
      this.loadCategory(this.slug);
    }
  }

  reload(): void {
    if (this.slug) {
      this.loadCategory(this.slug);
    }
  }

  private loadCategory(slug: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.category.set(null);
    this.articles.set([]);
    this.relatedCategories.set([]);

    this.apiService.getCategory(slug).subscribe({
      next: (category) => {
        this.category.set(category);
        this.titleService.setTitle(`${category.name} - Help Center`);
        this.breadcrumbItems.set([
          { label: 'Home', url: '/' },
          { label: category.name },
        ]);

        this.apiService.getCategoryArticles(slug).subscribe({
          next: (articles) => {
            this.articles.set(articles);
            this.loading.set(false);
          },
          error: () => {
            this.error.set('Failed to load articles for this category.');
            this.loading.set(false);
          },
        });

        this.apiService.getRelatedCategories(slug).subscribe({
          next: (related) => this.relatedCategories.set(related),
          error: () => this.relatedCategories.set([]),
        });
      },
      error: (err) => {
        const msg = (err?.message as string | undefined)?.startsWith('Category not found')
          ? 'Category not found.'
          : 'Failed to load category.';
        this.error.set(msg);
        this.loading.set(false);
      },
    });
  }
}
