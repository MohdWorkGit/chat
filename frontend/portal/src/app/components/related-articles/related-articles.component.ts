import { Component, ChangeDetectionStrategy, Input, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PortalApiService, ArticleSummary } from '../../services/portal-api.service';

@Component({
  selector: 'portal-related-articles',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    @if (relatedArticles().length > 0) {
      <section class="portal-related-articles">
        <h2 class="portal-related-articles-title">Related Articles</h2>
        <div class="portal-related-articles-grid">
          @for (article of relatedArticles(); track article.id) {
            <a [routerLink]="['/article', article.slug]" class="portal-related-article-card">
              <h3>{{ article.title }}</h3>
              <p>{{ article.description }}</p>
            </a>
          }
        </div>
      </section>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RelatedArticlesComponent implements OnInit {
  @Input() categorySlug = '';
  @Input() currentArticleId = 0;

  relatedArticles = signal<ArticleSummary[]>([]);

  constructor(private readonly apiService: PortalApiService) {}

  ngOnInit(): void {
    if (this.categorySlug) {
      this.apiService.getCategoryArticles(this.categorySlug).subscribe(articles => {
        const filtered = articles
          .filter(a => a.id !== this.currentArticleId)
          .slice(0, 3);
        this.relatedArticles.set(filtered);
      });
    }
  }
}
