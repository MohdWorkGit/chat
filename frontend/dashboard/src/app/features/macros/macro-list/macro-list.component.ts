import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MacrosActions } from '@store/macros/macros.actions';
import { selectAllMacros, selectMacrosLoading } from '@store/macros/macros.selectors';
import { Macro } from '@core/models/macro.model';
import { map } from 'rxjs';

@Component({
  selector: 'app-macro-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-6">
        <div>
          <h2 class="text-lg font-semibold text-gray-900">Macros</h2>
          <p class="text-sm text-gray-500 mt-1">Automate repetitive tasks with one-click macros.</p>
        </div>
        <button
          (click)="navigateToCreate()"
          class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
        >
          New Macro
        </button>
      </div>

      <!-- Search -->
      <div class="mb-4">
        <input
          [formControl]="searchControl"
          class="w-full max-w-sm rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          placeholder="Search macros..."
        />
      </div>

      <!-- Loading State -->
      @if (loading$ | async) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if ((filteredMacros$ | async); as macros) {
          <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
            @if (macros.length > 0) {
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Visibility</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Created</th>
                    <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider"></th>
                  </tr>
                </thead>
                <tbody class="bg-white divide-y divide-gray-200">
                  @for (macro of macros; track macro.id) {
                    <tr class="hover:bg-gray-50">
                      <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{{ macro.name }}</td>
                      <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {{ getActionsCount(macro) }} action{{ getActionsCount(macro) !== 1 ? 's' : '' }}
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap">
                        <span
                          class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium"
                          [class]="macro.visibility === 'global' ? 'bg-blue-100 text-blue-800' : 'bg-gray-100 text-gray-600'"
                        >
                          {{ macro.visibility === 'global' ? 'Global' : 'Personal' }}
                        </span>
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {{ macro.createdAt | date:'mediumDate' }}
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-right text-sm">
                        <button
                          (click)="navigateToEdit(macro.id)"
                          class="text-blue-600 hover:text-blue-800 mr-3"
                        >
                          Edit
                        </button>
                        <button
                          (click)="deleteMacro(macro.id)"
                          class="text-red-600 hover:text-red-800"
                        >
                          Delete
                        </button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            } @else {
              <div class="text-center py-12">
                <p class="text-sm text-gray-500">No macros found.</p>
                <p class="text-xs text-gray-400 mt-1">Create macros to automate repetitive tasks.</p>
              </div>
            }
          </div>
        }
      }
    </div>
  `,
  styles: [`:host { display: block; height: 100%; }`],
})
export class MacroListComponent implements OnInit {
  private store = inject(Store);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  macros$ = this.store.select(selectAllMacros);
  loading$ = this.store.select(selectMacrosLoading);

  searchControl = this.fb.control('');

  filteredMacros$ = this.macros$.pipe(
    map((macros) => {
      const query = (this.searchControl.value || '').toLowerCase();
      if (!query) return macros;
      return macros.filter((m) => m.name.toLowerCase().includes(query));
    })
  );

  ngOnInit(): void {
    this.store.dispatch(MacrosActions.loadMacros());
    this.searchControl.valueChanges.subscribe(() => {
      this.filteredMacros$ = this.macros$.pipe(
        map((macros) => {
          const query = (this.searchControl.value || '').toLowerCase();
          if (!query) return macros;
          return macros.filter((m) => m.name.toLowerCase().includes(query));
        })
      );
    });
  }

  getActionsCount(macro: Macro): number {
    if (!macro.actions) return 0;
    try {
      const parsed = JSON.parse(macro.actions);
      return Array.isArray(parsed) ? parsed.length : 0;
    } catch {
      return 0;
    }
  }

  navigateToCreate(): void {
    this.router.navigate(['/settings/macros/new']);
  }

  navigateToEdit(id: number): void {
    this.router.navigate(['/settings/macros', id]);
  }

  deleteMacro(id: number): void {
    if (confirm('Are you sure you want to delete this macro?')) {
      this.store.dispatch(MacrosActions.deleteMacro({ id }));
    }
  }
}
