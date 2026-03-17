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
      <div class="category-grid">
        @for (category of categories(); track category.id) {
          <a [routerLink]="['/category', category.slug]" class="category-card" style="text-decoration: none; color: inherit;">
            <div style="font-size: 2rem; margin-bottom: 12px;">{{ category.icon }}</div>
            <h3>{{ category.name }}</h3>
            <p>{{ category.description }}</p>
            <p style="margin-top: 8px; font-size: 0.8rem; color: var(--portal-primary);">
              {{ category.articleCount }} articles
            </p>
          </a>
        }
      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent implements OnInit {
  categories = signal<Category[]>([]);
  searchQuery = '';

  constructor(
    private readonly apiService: PortalApiService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.apiService.getCategories().subscribe(categories => {
      this.categories.set(categories);
    });
  }

  onSearch(): void {
    if (this.searchQuery.trim()) {
      this.router.navigate(['/search'], { queryParams: { q: this.searchQuery.trim() } });
    }
  }
}
