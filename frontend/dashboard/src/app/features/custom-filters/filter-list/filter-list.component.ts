import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { CustomFiltersActions } from '@store/custom-filters/custom-filters.actions';
import { selectAllCustomFilters, selectCustomFiltersLoading } from '@store/custom-filters/custom-filters.selectors';

@Component({
  selector: 'app-filter-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <div>
          <h2 class="text-lg font-semibold text-gray-900">Custom Filters</h2>
          <p class="text-sm text-gray-500 mt-1">Save and manage custom conversation filter views.</p>
        </div>
        <button
          (click)="showCreateForm = !showCreateForm"
          class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
        >
          {{ showCreateForm ? 'Cancel' : 'New Filter' }}
        </button>
      </div>

      @if (showCreateForm) {
        <div class="bg-white rounded-lg border border-gray-200 p-6 mb-6">
          <h3 class="text-sm font-medium text-gray-900 mb-4">Create Custom Filter</h3>
          <form [formGroup]="createForm" (ngSubmit)="createFilter()">
            <div class="space-y-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Filter Name</label>
                <input
                  formControlName="name"
                  class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="e.g., Unresolved high priority"
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Filter Type</label>
                <select
                  formControlName="filterType"
                  class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option value="conversation">Conversation</option>
                  <option value="contact">Contact</option>
                </select>
              </div>
              <div class="flex justify-end">
                <button
                  type="submit"
                  [disabled]="createForm.invalid"
                  class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 transition-colors"
                >
                  Save Filter
                </button>
              </div>
            </div>
          </form>
        </div>
      }

      @if (loading$ | async) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if ((filters$ | async); as filters) {
          <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
            @if (filters.length > 0) {
              <ul class="divide-y divide-gray-200">
                @for (filter of filters; track filter.id) {
                  <li class="px-6 py-4 hover:bg-gray-50 transition-colors">
                    <div class="flex items-center justify-between">
                      <div>
                        <p class="text-sm font-medium text-gray-900">{{ filter.name }}</p>
                        <p class="text-xs text-gray-500 mt-0.5">Type: {{ filter.filterType }}</p>
                      </div>
                      <div class="flex items-center gap-3">
                        <button
                          (click)="applyFilter(filter.id)"
                          class="text-sm text-blue-600 hover:text-blue-800"
                        >
                          Apply
                        </button>
                        <button
                          (click)="deleteFilter(filter.id)"
                          class="text-sm text-red-600 hover:text-red-800"
                        >
                          Delete
                        </button>
                      </div>
                    </div>
                  </li>
                }
              </ul>
            } @else {
              <div class="text-center py-12">
                <p class="text-sm text-gray-500">No custom filters saved yet.</p>
                <p class="text-xs text-gray-400 mt-1">Create a filter to quickly access specific conversation views.</p>
              </div>
            }
          </div>
        }
      }
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
    }
  `],
})
export class FilterListComponent implements OnInit {
  private store = inject(Store);
  private fb = inject(FormBuilder);

  filters$ = this.store.select(selectAllCustomFilters);
  loading$ = this.store.select(selectCustomFiltersLoading);

  showCreateForm = false;

  createForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    filterType: ['conversation'],
  });

  ngOnInit(): void {
    this.store.dispatch(CustomFiltersActions.loadCustomFilters());
  }

  createFilter(): void {
    if (this.createForm.invalid) return;
    this.store.dispatch(
      CustomFiltersActions.createCustomFilter({
        data: {
          name: this.createForm.value.name,
          filterType: this.createForm.value.filterType,
          query: '{}',
        },
      })
    );
    this.createForm.reset({ name: '', filterType: 'conversation' });
    this.showCreateForm = false;
  }

  applyFilter(filterId: number): void {
    this.store.dispatch(CustomFiltersActions.selectCustomFilter({ id: filterId }));
  }

  deleteFilter(id: number): void {
    if (confirm('Are you sure you want to delete this filter?')) {
      this.store.dispatch(CustomFiltersActions.deleteCustomFilter({ id }));
    }
  }
}
