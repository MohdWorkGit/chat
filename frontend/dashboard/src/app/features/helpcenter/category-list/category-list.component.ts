import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HelpCenterService } from '@core/services/helpcenter.service';
import { Category } from '@core/models/helpcenter.model';
import { HelpCenterTabsComponent } from '../helpcenter-tabs/helpcenter-tabs.component';
import { BehaviorSubject, switchMap, of } from 'rxjs';

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [CommonModule, RouterLink, HelpCenterTabsComponent],
  template: `
    <app-helpcenter-tabs />
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-lg font-semibold text-gray-900">Categories</h2>
        <a
          routerLink="new"
          class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
        >
          New Category
        </a>
      </div>

      @if (loading$ | async) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else if (portalError) {
        <div class="bg-red-50 border border-red-200 rounded-lg p-4">
          <p class="text-sm text-red-700">No portal found. Create a portal before managing categories.</p>
        </div>
      } @else {
        @if ((categories$ | async); as categories) {
          @if (categories.length === 0) {
            <div class="text-center py-12">
              <p class="text-sm text-gray-500">No categories yet.</p>
              <a
                routerLink="new"
                class="mt-4 inline-block px-4 py-2 text-sm font-medium text-blue-600 hover:text-blue-700"
              >
                Create your first category
              </a>
            </div>
          } @else {
            <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Slug</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Description</th>
                    <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Position</th>
                    <th class="px-6 py-3"></th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-gray-200">
                  @for (category of categories; track category.id) {
                    <tr class="hover:bg-gray-50 transition-colors">
                      <td class="px-6 py-4">
                        <a [routerLink]="[category.id]" class="text-sm font-medium text-gray-900 hover:text-blue-600">
                          {{ category.name }}
                        </a>
                      </td>
                      <td class="px-6 py-4 text-sm text-gray-500">{{ category.slug }}</td>
                      <td class="px-6 py-4 text-sm text-gray-500">{{ category.description || '—' }}</td>
                      <td class="px-6 py-4 text-sm text-gray-500 text-right">{{ category.position }}</td>
                      <td class="px-6 py-4 text-right">
                        <button
                          (click)="deleteCategory(category)"
                          class="text-sm text-red-600 hover:text-red-800"
                        >
                          Delete
                        </button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }
        }
      }
    </div>
  `,
  styles: [`
    :host { display: block; height: 100%; overflow-y: auto; }
  `],
})
export class CategoryListComponent implements OnInit {
  private readonly helpCenter = inject(HelpCenterService);

  categories$ = new BehaviorSubject<Category[]>([]);
  loading$ = new BehaviorSubject<boolean>(false);
  portalError = false;
  private portalId: number | null = null;

  ngOnInit(): void {
    this.loadCategories();
  }

  private loadCategories(): void {
    this.loading$.next(true);
    this.helpCenter.getPortals().pipe(
      switchMap((portals) => {
        if (!portals || portals.length === 0) {
          this.portalError = true;
          return of([] as Category[]);
        }
        this.portalId = portals[0].id;
        return this.helpCenter.getCategories(this.portalId);
      })
    ).subscribe({
      next: (categories) => {
        this.categories$.next(categories ?? []);
        this.loading$.next(false);
      },
      error: () => {
        this.loading$.next(false);
      },
    });
  }

  deleteCategory(category: Category): void {
    if (!this.portalId) return;
    if (!confirm(`Delete category "${category.name}"?`)) return;

    this.helpCenter.deleteCategory(this.portalId, category.id).subscribe({
      next: () => this.loadCategories(),
    });
  }
}
