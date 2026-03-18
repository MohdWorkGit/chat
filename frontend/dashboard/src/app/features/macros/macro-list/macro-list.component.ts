import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { MacrosActions } from '@store/macros/macros.actions';
import { selectAllMacros, selectMacrosLoading } from '@store/macros/macros.selectors';
import { Macro } from '@core/models/macro.model';

@Component({
  selector: 'app-macro-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-lg font-semibold text-gray-900">Macros</h2>
        <button
          (click)="showCreateForm = !showCreateForm"
          class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
        >
          {{ showCreateForm ? 'Cancel' : 'New Macro' }}
        </button>
      </div>

      @if (showCreateForm) {
        <div class="bg-white rounded-lg border border-gray-200 p-6 mb-6">
          <h3 class="text-sm font-semibold text-gray-900 mb-4">Create Macro</h3>
          <form [formGroup]="createForm" (ngSubmit)="createMacro()">
            <div class="grid grid-cols-2 gap-4 mb-4">
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">Name</label>
                <input
                  formControlName="name"
                  class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="Macro name"
                />
              </div>
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">Visibility</label>
                <select
                  formControlName="visibility"
                  class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option value="personal">Personal</option>
                  <option value="global">Global</option>
                </select>
              </div>
            </div>
            <div class="flex justify-end">
              <button
                type="submit"
                [disabled]="createForm.invalid"
                class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 transition-colors"
              >
                Create
              </button>
            </div>
          </form>
        </div>
      }

      @if (loading$ | async) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if ((macros$ | async); as macros) {
          <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
            @if (macros.length > 0) {
              <ul class="divide-y divide-gray-200">
                @for (macro of macros; track macro.id) {
                  <li class="px-6 py-4 hover:bg-gray-50 transition-colors">
                    <div class="flex items-center justify-between">
                      <div class="flex-1">
                        <div class="flex items-center gap-3">
                          <span class="text-sm font-medium text-gray-900">{{ macro.name }}</span>
                          <span
                            class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium"
                            [class]="macro.visibility === 'global' ? 'bg-blue-100 text-blue-800' : 'bg-gray-100 text-gray-600'"
                          >
                            {{ macro.visibility === 'global' ? 'Global' : 'Personal' }}
                          </span>
                        </div>
                        <p class="text-xs text-gray-400 mt-1">
                          Created {{ macro.createdAt | date:'medium' }}
                        </p>
                      </div>
                      <div class="flex items-center gap-3">
                        <button
                          (click)="deleteMacro(macro.id)"
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
                <p class="text-sm text-gray-500">No macros created yet.</p>
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
  private fb = inject(FormBuilder);

  macros$ = this.store.select(selectAllMacros);
  loading$ = this.store.select(selectMacrosLoading);

  showCreateForm = false;

  createForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    visibility: ['personal', Validators.required],
  });

  ngOnInit(): void {
    this.store.dispatch(MacrosActions.loadMacros());
  }

  createMacro(): void {
    if (this.createForm.invalid) return;
    this.store.dispatch(MacrosActions.createMacro({ data: this.createForm.value }));
    this.createForm.reset({ visibility: 'personal' });
    this.showCreateForm = false;
  }

  deleteMacro(id: number): void {
    if (confirm('Are you sure you want to delete this macro?')) {
      this.store.dispatch(MacrosActions.deleteMacro({ id }));
    }
  }
}
