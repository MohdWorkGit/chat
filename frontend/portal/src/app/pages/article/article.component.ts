import { Component, ChangeDetectionStrategy, OnInit, Input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Meta, Title } from '@angular/platform-browser';
import { PortalApiService, Article, TocEntry } from '../../services/portal-api.service';
import { BreadcrumbComponent, BreadcrumbItem } from '../../components/breadcrumb/breadcrumb.component';
import { RelatedArticlesComponent } from '../../components/related-articles/related-articles.component';

@Component({
  selector: 'portal-article',
  standalone: true,
  imports: [CommonModule, RouterLink, BreadcrumbComponent, RelatedArticlesComponent],
  template: `
    <div class="container" style="padding-top: 16px;">
      @if (article(); as article) {
        <portal-breadcrumb [items]="breadcrumbItems()" />

        <div class="article-layout">
          <article class="article-content">
            <h1>{{ article.title }}</h1>
            <div style="color: var(--portal-text-secondary); font-size: 0.875rem; margin-bottom: 24px;">
              Last updated: {{ article.updatedAt | date:'mediumDate' }}
            </div>
            <div [innerHTML]="article.contentHtml"></div>
          </article>

          @if (tableOfContents().length > 0) {
            <aside class="toc article-toc">
              <div class="toc-title">On this page</div>
              @for (entry of tableOfContents(); track entry.id) {
                <a
                  [href]="'#' + entry.id"
                  [style.padding-left.px]="(entry.level - 2) * 12">
                  {{ entry.text }}
                </a>
              }
            </aside>
          }
        </div>

        <portal-related-articles
          [categorySlug]="article.category?.slug || ''"
          [currentArticleId]="article.id" />
      } @else {
        <div style="text-align: center; padding: 64px 0;">
          <p style="color: var(--portal-text-secondary);">Loading article...</p>
        </div>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArticleComponent implements OnInit {
  @Input() slug = '';

  article = signal<Article | null>(null);
  tableOfContents = signal<TocEntry[]>([]);
  breadcrumbItems = signal<BreadcrumbItem[]>([]);

  constructor(
    private readonly apiService: PortalApiService,
    private readonly titleService: Title,
    private readonly metaService: Meta,
  ) {}

  ngOnInit(): void {
    if (this.slug) {
      this.loadArticle(this.slug);
    }
  }

  private loadArticle(slug: string): void {
    this.apiService.getArticle(slug).subscribe(article => {
      this.article.set(article);
      this.titleService.setTitle(`${article.title} - Help Center`);
      this.metaService.updateTag({ name: 'description', content: article.description });
      this.tableOfContents.set(this.extractToc(article.contentHtml));

      const crumbs: BreadcrumbItem[] = [{ label: 'Home', url: '/' }];
      if (article.category) {
        crumbs.push({ label: article.category.name, url: `/category/${article.category.slug}` });
      }
      crumbs.push({ label: article.title });
      this.breadcrumbItems.set(crumbs);
    });
  }

  private extractToc(html: string): TocEntry[] {
    const entries: TocEntry[] = [];
    const regex = /<h([2-4])\s+id="([^"]+)"[^>]*>([^<]+)<\/h[2-4]>/g;
    let match: RegExpExecArray | null;
    while ((match = regex.exec(html)) !== null) {
      entries.push({
        level: parseInt(match[1], 10),
        id: match[2],
        text: match[3],
      });
    }
    return entries;
  }
}
