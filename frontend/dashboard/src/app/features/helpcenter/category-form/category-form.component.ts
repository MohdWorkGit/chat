import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HelpCenterService } from '@core/services/helpcenter.service';
import { HelpCenterTabsComponent } from '../helpcenter-tabs/helpcenter-tabs.component';
import { switchMap, of } from 'rxjs';

@Component({
  selector: 'app-category-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, HelpCenterTabsComponent],
  template: `
    <app-helpcenter-tabs />
    <div class="p-6 max-w-2xl mx-auto">
      <div class="mb-6">
        <button
          (click)="goBack()"
          class="text-sm text-blue-600 hover:text-blue-800 mb-2 inline-flex items-center gap-1"
        >
          <svg class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5L8.25 12l7.5-7.5" />
          </svg>
          Back to Categories
        </button>
        <h2 class="text-lg font-semibold text-gray-900">{{ isEditing ? 'Edit Category' : 'New Category' }}</h2>
      </div>

      @if (portalError) {
        <div class="bg-red-50 border border-red-200 rounded-lg p-4">
          <p class="text-sm text-red-700">No portal found. Create a portal before managing categories.</p>
        </div>
      } @else {
        <div class="bg-white rounded-lg border border-gray-200 p-6">
          <form [formGroup]="categoryForm" (ngSubmit)="onSubmit()" class="space-y-5">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Name <span class="text-red-500">*</span></label>
              <input
                formControlName="name"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="Getting Started"
              />
              @if (categoryForm.get('name')?.invalid && categoryForm.get('name')?.touched) {
                <p class="mt-1 text-xs text-red-600">Name is required.</p>
              }
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
              <textarea
                formControlName="description"
                rows="3"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="What this category is about"
              ></textarea>
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Locale</label>
                <input
                  formControlName="locale"
                  class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="en"
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Position</label>
                <input
                  type="number"
                  formControlName="position"
                  class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>
            </div>

            @if (error) {
              <div class="rounded-lg bg-red-50 border border-red-200 px-4 py-3">
                <p class="text-sm text-red-700">{{ error }}</p>
              </div>
            }

            <div class="flex items-center gap-3 pt-2">
              <button
                type="submit"
                [disabled]="categoryForm.invalid || saving"
                class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 rounded-lg transition-colors"
              >
                {{ saving ? 'Saving...' : (isEditing ? 'Save Changes' : 'Create Category') }}
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
    :host { display: block; height: 100%; overflow-y: auto; }
  `],
})
export class CategoryFormComponent implements OnInit {
  private readonly helpCenter = inject(HelpCenterService);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  isEditing = false;
  saving = false;
  error: string | null = null;
  portalError = false;

  private portalId: number | null = null;
  private categoryId: number | null = null;

  categoryForm: FormGroup = this.fb.group({
    name: ['', [Validators.required]],
    description: [''],
    locale: ['en'],
    position: [0],
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditing = true;
      this.categoryId = +id;
    }

    this.helpCenter.getPortals().pipe(
      switchMap((portals) => {
        if (!portals || portals.length === 0) {
          this.portalError = true;
          return of(null);
        }
        this.portalId = portals[0].id;

        if (this.isEditing && this.categoryId) {
          return this.helpCenter.getCategory(this.portalId, this.categoryId);
        }
        return of(null);
      })
    ).subscribe({
      next: (category) => {
        if (category) {
          this.categoryForm.patchValue({
            name: category.name,
            description: category.description ?? '',
            position: category.position ?? 0,
          });
        }
      },
      error: () => {
        this.error = 'Failed to load category data.';
      },
    });
  }

  onSubmit(): void {
    if (this.categoryForm.invalid || !this.portalId) return;

    this.saving = true;
    this.error = null;
    const payload = this.categoryForm.value;

    const request$ = this.isEditing && this.categoryId
      ? this.helpCenter.updateCategory(this.portalId, this.categoryId, payload)
      : this.helpCenter.createCategory(this.portalId, payload);

    request$.subscribe({
      next: () => {
        this.saving = false;
        this.router.navigate(['/helpcenter/categories']);
      },
      error: () => {
        this.saving = false;
        this.error = 'Failed to save category. Please try again.';
      },
    });
  }

  goBack(): void {
    this.router.navigate(['/helpcenter/categories']);
  }
}
