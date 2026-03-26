import { Component, ChangeDetectionStrategy, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PortalApiService, ArticleSummary } from '../../services/portal-api.service';

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

        @if (hasSearched()) {
          <div class="portal-search-meta">
            <p style="color: var(--portal-text-secondary); margin-bottom: 24px;">
              Showing {{ rangeStart() }}-{{ rangeEnd() }} of {{ totalResults() }} result(s) for "{{ lastQuery() }}"
            </p>
          </div>

          <ul class="article-list">
            @for (article of paginatedResults(); track article.id) {
              <li class="article-list-item">
                <a [routerLink]="['/article', article.slug]">{{ article.title }}</a>
                <p>{{ article.description }}</p>
                @if (article.categoryName) {
                  <span style="display: inline-block; margin-top: 4px; padding: 2px 8px; background: #f3f4f6; border-radius: 4px; font-size: 0.75rem; color: var(--portal-text-secondary);">
                    {{ article.categoryName }}
                  </span>
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

          @if (totalPages() > 1) {
            <div class="portal-search-pagination">
              <button
                class="portal-pagination-btn"
                [disabled]="currentPage() <= 1"
                (click)="goToPage(currentPage() - 1)">
                Previous
              </button>
              <span class="portal-pagination-info">
                Page {{ currentPage() }} of {{ totalPages() }}
              </span>
              <button
                class="portal-pagination-btn"
                [disabled]="currentPage() >= totalPages()"
                (click)="goToPage(currentPage() + 1)">
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
  allResults = signal<ArticleSummary[]>([]);
  hasSearched = signal(false);
  lastQuery = signal('');
  currentPage = signal(1);

  totalResults = computed(() => this.allResults().length);
  totalPages = computed(() => Math.ceil(this.totalResults() / RESULTS_PER_PAGE));

  rangeStart = computed(() => {
    if (this.totalResults() === 0) return 0;
    return (this.currentPage() - 1) * RESULTS_PER_PAGE + 1;
  });

  rangeEnd = computed(() =>
    Math.min(this.currentPage() * RESULTS_PER_PAGE, this.totalResults()),
  );

  paginatedResults = computed(() => {
    const start = (this.currentPage() - 1) * RESULTS_PER_PAGE;
    return this.allResults().slice(start, start + RESULTS_PER_PAGE);
  });

  constructor(
    private readonly apiService: PortalApiService,
    private readonly route: ActivatedRoute,
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const q = params['q'];
      if (q) {
        this.searchQuery = q;
        this.performSearch(q);
      }
    });
  }

  onSearch(): void {
    const query = this.searchQuery.trim();
    if (query) {
      this.currentPage.set(1);
      this.performSearch(query);
    }
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
    }
  }

  private performSearch(query: string): void {
    this.lastQuery.set(query);
    this.apiService.searchArticles(query).subscribe(results => {
      this.allResults.set(results);
      this.hasSearched.set(true);
    });
  }
}
