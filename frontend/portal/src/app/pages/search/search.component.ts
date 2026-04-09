import { Component, ChangeDetectionStrategy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PortalApiService, ArticleSummary, PageMeta } from '../../services/portal-api.service';

const RESULTS_PER_PAGE = 10;

@Component({
  selector: 'portal-search',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="container" style="padding-top: 32px;">
      <div style="max-width: 800px; margin: 0 auto;">
        <div style="margin-bottom: 32px;">
          <input
            class="search-input"
            type="text"
            placeholder="Search for articles..."
            [(ngModel)]="searchQuery"
            (keydown.enter)="onSearch()"
            style="width: 100%; box-shadow: 0 1px 4px rgba(0,0,0,0.06); border: 1px solid var(--portal-border);"
            aria-label="Search articles" />
        </div>

        @if (loading()) {
          <div style="padding: 48px 0; text-align: center;">
            <p style="color: var(--portal-text-secondary);">Searching…</p>
          </div>
        } @else if (error()) {
          <div style="padding: 48px 0; text-align: center;">
            <p style="color: #b91c1c;">{{ error() }}</p>
            <button class="portal-pagination-btn" style="margin-top: 12px;" (click)="onSearch()">Try again</button>
          </div>
        } @else if (hasSearched()) {
          <div class="portal-search-meta">
            <p style="color: var(--portal-text-secondary); margin-bottom: 24px;">
              Showing {{ rangeStart() }}-{{ rangeEnd() }} of {{ meta().totalCount }} result(s) for "{{ lastQuery() }}"
            </p>
          </div>

          <ul class="article-list">
            @for (article of results(); track article.id) {
              <li class="article-list-item">
                <a [routerLink]="['/article', article.slug]">{{ article.title }}</a>
                @if (article.description) {
                  <p>{{ article.description }}</p>
                }
              </li>
            } @empty {
              <li style="padding: 48px 0; text-align: center;">
                <p style="color: var(--portal-text-secondary); font-size: 1.125rem;">No results found</p>
                <p style="color: var(--portal-text-secondary); font-size: 0.875rem; margin-top: 8px;">
                  Try different keywords or browse our categories.
                </p>
              </li>
            }
          </ul>

          @if (meta().totalPages > 1) {
            <div class="portal-search-pagination">
              <button
                class="portal-pagination-btn"
                [disabled]="meta().page <= 1"
                (click)="goToPage(meta().page - 1)">
                Previous
              </button>
              <span class="portal-pagination-info">
                Page {{ meta().page }} of {{ meta().totalPages }}
              </span>
              <button
                class="portal-pagination-btn"
                [disabled]="meta().page >= meta().totalPages"
                (click)="goToPage(meta().page + 1)">
                Next
              </button>
            </div>
          }
        }
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchComponent implements OnInit {
  searchQuery = '';
  results = signal<ArticleSummary[]>([]);
  meta = signal<PageMeta>({ totalCount: 0, page: 1, pageSize: RESULTS_PER_PAGE, totalPages: 0 });
  hasSearched = signal(false);
  loading = signal(false);
  error = signal<string | null>(null);
  lastQuery = signal('');

  constructor(
    private readonly apiService: PortalApiService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const q = params['q'];
      const page = Number(params['page']) || 1;
      if (q) {
        this.searchQuery = q;
        this.performSearch(q, page);
      }
    });
  }

  onSearch(): void {
    const query = this.searchQuery.trim();
    if (!query) return;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { q: query, page: 1 },
      queryParamsHandling: 'merge',
    });
  }

  goToPage(page: number): void {
    if (page < 1) return;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { q: this.lastQuery() || this.searchQuery, page },
      queryParamsHandling: 'merge',
    });
  }

  rangeStart(): number {
    if (this.meta().totalCount === 0) return 0;
    return (this.meta().page - 1) * this.meta().pageSize + 1;
  }

  rangeEnd(): number {
    return Math.min(this.meta().page * this.meta().pageSize, this.meta().totalCount);
  }

  private performSearch(query: string, page: number): void {
    this.lastQuery.set(query);
    this.loading.set(true);
    this.error.set(null);
    this.apiService.searchArticles(query, page, RESULTS_PER_PAGE).subscribe({
      next: (result) => {
        this.results.set(result.items);
        this.meta.set(result.meta);
        this.hasSearched.set(true);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Something went wrong while searching. Please try again.');
        this.loading.set(false);
      },
    });
  }
}
