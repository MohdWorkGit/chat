import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HelpCenterService } from '@core/services/helpcenter.service';
import { Category } from '@core/models/helpcenter.model';
import { HelpCenterTabsComponent } from '../helpcenter-tabs/helpcenter-tabs.component';
import { forkJoin, switchMap, of } from 'rxjs';

type StatusLabel = 'draft' | 'published' | 'archived';
const STATUS_TO_INT: Record<StatusLabel, number> = { draft: 0, published: 1, archived: 2 };
const INT_TO_STATUS: StatusLabel[] = ['draft', 'published', 'archived'];

@Component({
  selector: 'app-article-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, HelpCenterTabsComponent],
  template: `
    <app-helpcenter-tabs />
    <div class="p-6 max-w-4xl mx-auto">
      <!-- Header -->
      <div class="mb-6">
        <button
          (click)="goBack()"
          class="text-sm text-blue-600 hover:text-blue-800 mb-2 inline-flex items-center gap-1"
        >
          <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5L8.25 12l7.5-7.5" />
          </svg>
          Back to Articles
        </button>
        <h2 class="text-lg font-semibold text-gray-900">{{ isEditing ? 'Edit Article' : 'New Article' }}</h2>
      </div>

      @if (portalError) {
        <div class="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
          <p class="text-sm text-red-700">No portal found. Please create a portal before writing articles.</p>
        </div>
      } @else {
        <div class="bg-white rounded-lg border border-gray-200 p-6">
          <form [formGroup]="articleForm" (ngSubmit)="onSubmit()" class="space-y-5">
            <!-- Title -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Title <span class="text-red-500">*</span></label>
              <input
                formControlName="title"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="Article title"
              />
              @if (articleForm.get('title')?.invalid && articleForm.get('title')?.touched) {
                <p class="mt-1 text-xs text-red-600">Title is required.</p>
              }
            </div>

            <!-- Description -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Short Description</label>
              <input
                formControlName="description"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="Brief summary shown in search results"
              />
            </div>

            <!-- Content -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Content <span class="text-red-500">*</span></label>
              <textarea
                formControlName="content"
                rows="16"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm font-mono focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="Write your article content here..."
              ></textarea>
              @if (articleForm.get('content')?.invalid && articleForm.get('content')?.touched) {
                <p class="mt-1 text-xs text-red-600">Content is required.</p>
              }
            </div>

            <!-- Category -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Category</label>
              <select
                formControlName="categoryId"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              >
                <option [ngValue]="null">— Uncategorized —</option>
                @for (cat of categories; track cat.id) {
                  <option [ngValue]="cat.id">{{ cat.name }}</option>
                }
              </select>
              @if (categories.length === 0) {
                <p class="mt-1 text-xs text-gray-500">
                  No categories yet.
                  <a routerLink="/helpcenter/categories/new" class="text-blue-600 hover:text-blue-800">Create one</a>.
                </p>
              }
            </div>

            <!-- Status -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Status</label>
              <select
                formControlName="status"
                class="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              >
                <option value="draft">Draft</option>
                <option value="published">Published</option>
                <option value="archived">Archived</option>
              </select>
            </div>

            <!-- Error -->
            @if (error) {
              <div class="rounded-lg bg-red-50 border border-red-200 px-4 py-3">
                <p class="text-sm text-red-700">{{ error }}</p>
              </div>
            }

            <!-- Actions -->
            <div class="flex items-center gap-3 pt-2">
              <button
                type="submit"
                [disabled]="articleForm.invalid || saving"
                class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed rounded-lg transition-colors"
              >
                {{ saving ? 'Saving...' : (isEditing ? 'Save Changes' : 'Create Article') }}
              </button>
              <button
                type="button"
                (click)="goBack()"
                class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 hover:bg-gray-50 rounded-lg transition-colors"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      }
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
      overflow-y: auto;
    }
  `],
})
export class ArticleFormComponent implements OnInit {
  private readonly helpCenter = inject(HelpCenterService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  isEditing = false;
  saving = false;
  error: string | null = null;
  portalError = false;
  categories: Category[] = [];

  private portalId: number | null = null;
  private articleId: number | null = null;

  articleForm: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(1)]],
    description: [''],
    content: ['', Validators.required],
    status: ['draft'],
    categoryId: [null as number | null],
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditing = true;
      this.articleId = +id;
    }

    this.helpCenter.getPortals().pipe(
      switchMap((portals) => {
        if (!portals || portals.length === 0) {
          this.portalError = true;
          return of({ categories: [] as Category[], article: null as any });
        }
        this.portalId = portals[0].id;

        const categories$ = this.helpCenter.getCategories(this.portalId);
        const article$ = this.isEditing && this.articleId
          ? this.helpCenter.getArticle(this.portalId, this.articleId)
          : of(null);

        return forkJoin({ categories: categories$, article: article$ });
      })
    ).subscribe({
      next: ({ categories, article }) => {
        this.categories = categories ?? [];
        if (article) {
          const statusValue = typeof article.status === 'number'
            ? INT_TO_STATUS[article.status] ?? 'draft'
            : (article.status ?? 'draft');

          this.articleForm.patchValue({
            title: article.title,
            description: article.description ?? '',
            content: article.content ?? '',
            status: statusValue,
            categoryId: article.categoryId ?? null,
          });
        }
      },
      error: () => {
        this.error = 'Failed to load article data.';
      },
    });
  }

  onSubmit(): void {
    if (this.articleForm.invalid || !this.portalId) return;

    this.saving = true;
    this.error = null;
    const raw = this.articleForm.value;
    const payload = {
      title: raw.title,
      description: raw.description ?? '',
      content: raw.content ?? '',
      status: STATUS_TO_INT[raw.status as StatusLabel] ?? 0,
      categoryId: raw.categoryId ?? null,
    };

    const request$ = this.isEditing && this.articleId
      ? this.helpCenter.updateArticle(this.portalId, this.articleId, payload as any)
      : this.helpCenter.createArticle(this.portalId, payload as any);

    request$.subscribe({
      next: () => {
        this.saving = false;
        this.router.navigate(['/helpcenter/articles']);
      },
      error: () => {
        this.saving = false;
        this.error = 'Failed to save article. Please try again.';
      },
    });
  }

  goBack(): void {
    this.router.navigate(['/helpcenter/articles']);
  }
}
