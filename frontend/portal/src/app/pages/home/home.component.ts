import { Component, ChangeDetectionStrategy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PortalApiService, Category } from '../../services/portal-api.service';

@Component({
  selector: 'portal-home',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <section class="hero-section">
      <div class="container">
        <h1>How can we help?</h1>
        <p>Search our knowledge base or browse categories below.</p>
        <input
          class="search-input"
          type="text"
          placeholder="Search for articles..."
          [(ngModel)]="searchQuery"
          (keydown.enter)="onSearch()"
          aria-label="Search articles" />
      </div>
    </section>

    <section class="container">
      @if (loading()) {
        <div style="padding: 48px 0; text-align: center;">
          <p style="color: var(--portal-text-secondary);">Loading categories…</p>
        </div>
      } @else if (error()) {
        <div style="padding: 48px 0; text-align: center;">
          <p style="color: #b91c1c;">{{ error() }}</p>
          <button class="portal-pagination-btn" style="margin-top: 12px;" (click)="loadCategories()">Try again</button>
        </div>
      } @else {
        <div class="category-grid">
          @for (category of categories(); track category.id) {
            <a [routerLink]="['/category', category.slug]" class="category-card" style="text-decoration: none; color: inherit;">
              <h3>{{ category.name }}</h3>
              @if (category.description) {
                <p>{{ category.description }}</p>
              }
              <p style="margin-top: 8px; font-size: 0.8rem; color: var(--portal-primary);">
                {{ category.articleCount }} {{ category.articleCount === 1 ? 'article' : 'articles' }}
              </p>
            </a>
          } @empty {
            <p style="padding: 24px; color: var(--portal-text-secondary);">
              No categories are available yet.
            </p>
          }
        </div>
      }
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent implements OnInit {
  categories = signal<Category[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  searchQuery = '';

  constructor(
    private readonly apiService: PortalApiService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.loading.set(true);
    this.error.set(null);
    this.apiService.getCategories().subscribe({
      next: (categories) => {
        this.categories.set(categories);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load categories.');
        this.loading.set(false);
      },
    });
  }

  onSearch(): void {
    if (this.searchQuery.trim()) {
      this.router.navigate(['/search'], { queryParams: { q: this.searchQuery.trim() } });
    }
  }
}
