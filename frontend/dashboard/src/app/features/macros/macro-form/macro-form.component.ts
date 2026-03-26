import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { MacrosActions } from '@store/macros/macros.actions';
import { selectMacroEntities, selectMacrosLoading } from '@store/macros/macros.selectors';
import { first } from 'rxjs';

const ACTION_TYPES = [
  { value: 'assign_team', label: 'Assign Team' },
  { value: 'assign_agent', label: 'Assign Agent' },
  { value: 'add_label', label: 'Add Label' },
  { value: 'remove_label', label: 'Remove Label' },
  { value: 'change_status', label: 'Change Status' },
  { value: 'change_priority', label: 'Change Priority' },
  { value: 'send_message', label: 'Send Message' },
  { value: 'mute_conversation', label: 'Mute Conversation' },
  { value: 'snooze_conversation', label: 'Snooze Conversation' },
];

const ACTION_PARAM_LABELS: Record<string, string> = {
  assign_team: 'Team ID',
  assign_agent: 'Agent ID',
  add_label: 'Label name',
  remove_label: 'Label name',
  change_status: 'Status (open, resolved, pending)',
  change_priority: 'Priority (low, medium, high, urgent)',
  send_message: 'Message content',
  mute_conversation: '',
  snooze_conversation: 'Snooze until (hours)',
};

@Component({
  selector: 'app-macro-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-6">
        <div>
          <button
            (click)="goBack()"
            class="text-sm text-blue-600 hover:text-blue-800 mb-2 inline-flex items-center gap-1"
          >
            <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5L8.25 12l7.5-7.5" />
            </svg>
            Back to Macros
          </button>
          <h2 class="text-lg font-semibold text-gray-900">{{ isEditing ? 'Edit Macro' : 'New Macro' }}</h2>
        </div>
      </div>

      <div class="bg-white rounded-lg border border-gray-200 p-6">
        <form [formGroup]="macroForm" (ngSubmit)="onSubmit()" class="space-y-6">
          <!-- Name -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Name</label>
            <input
              formControlName="name"
              class="w-full max-w-md rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="Enter macro name"
            />
          </div>

          <!-- Visibility -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Visibility</label>
            <div class="flex items-center gap-6">
              <label class="flex items-center gap-2 cursor-pointer">
                <input
                  type="radio"
                  formControlName="visibility"
                  value="personal"
                  class="h-4 w-4 border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <span class="text-sm text-gray-700">Personal</span>
              </label>
              <label class="flex items-center gap-2 cursor-pointer">
                <input
                  type="radio"
                  formControlName="visibility"
                  value="global"
                  class="h-4 w-4 border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <span class="text-sm text-gray-700">Global</span>
              </label>
            </div>
          </div>

          <!-- Actions Builder -->
          <div>
            <div class="flex items-center justify-between mb-3">
              <label class="block text-sm font-medium text-gray-700">Actions</label>
              <button
                type="button"
                (click)="addAction()"
                class="px-3 py-1.5 text-xs font-medium text-blue-600 border border-blue-300 rounded-lg hover:bg-blue-50 transition-colors"
              >
                + Add Action
              </button>
            </div>

            @if (actionsArray.length === 0) {
              <div class="text-center py-6 border border-dashed border-gray-300 rounded-lg">
                <p class="text-sm text-gray-500">No actions added yet.</p>
                <p class="text-xs text-gray-400 mt-1">Add actions to define what this macro does.</p>
              </div>
            }

            <div class="space-y-3" formArrayName="actions">
              @for (action of actionsArray.controls; track $index; let i = $index) {
                <div class="flex items-start gap-3 p-4 border border-gray-200 rounded-lg bg-gray-50" [formGroupName]="i">
                  <!-- Reorder buttons -->
                  <div class="flex flex-col gap-1 pt-1">
                    <button
                      type="button"
                      [disabled]="i === 0"
                      (click)="moveAction(i, i - 1)"
                      class="text-gray-400 hover:text-gray-600 disabled:opacity-30"
                    >
                      <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M4.5 15.75l7.5-7.5 7.5 7.5" />
                      </svg>
                    </button>
                    <button
                      type="button"
                      [disabled]="i === actionsArray.length - 1"
                      (click)="moveAction(i, i + 1)"
                      class="text-gray-400 hover:text-gray-600 disabled:opacity-30"
                    >
                      <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M19.5 8.25l-7.5 7.5-7.5-7.5" />
                      </svg>
                    </button>
                  </div>

                  <!-- Action Type -->
                  <div class="flex-1">
                    <div class="grid grid-cols-2 gap-3">
                      <div>
                        <label class="block text-xs font-medium text-gray-600 mb-1">Type</label>
                        <select
                          formControlName="type"
                          class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                        >
                          <option value="">Select action type</option>
                          @for (actionType of actionTypes; track actionType.value) {
                            <option [value]="actionType.value">{{ actionType.label }}</option>
                          }
                        </select>
                      </div>
                      @if (getParamLabel(action.get('type')?.value)) {
                        <div>
                          <label class="block text-xs font-medium text-gray-600 mb-1">
                            {{ getParamLabel(action.get('type')?.value) }}
                          </label>
                          @if (action.get('type')?.value === 'send_message') {
                            <textarea
                              formControlName="params"
                              rows="2"
                              class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                              placeholder="Enter value"
                            ></textarea>
                          } @else {
                            <input
                              formControlName="params"
                              class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                              placeholder="Enter value"
                            />
                          }
                        </div>
                      }
                    </div>
                  </div>

                  <!-- Remove -->
                  <button
                    type="button"
                    (click)="removeAction(i)"
                    class="mt-5 text-red-400 hover:text-red-600"
                  >
                    <svg class="h-5 w-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
              }
            </div>
          </div>

          <!-- Submit -->
          <div class="flex justify-end gap-3 pt-4 border-t border-gray-200">
            <button
              type="button"
              (click)="goBack()"
              class="px-4 py-2 text-sm font-medium text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              [disabled]="macroForm.invalid || (loading$ | async)"
              class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              {{ isEditing ? 'Update Macro' : 'Create Macro' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`:host { display: block; height: 100%; }`],
})
export class MacroFormComponent implements OnInit {
  private store = inject(Store);
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  loading$ = this.store.select(selectMacrosLoading);
  macroEntities$ = this.store.select(selectMacroEntities);

  actionTypes = ACTION_TYPES;
  isEditing = false;
  editingId: number | null = null;

  macroForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    visibility: ['personal', Validators.required],
    actions: this.fb.array([]),
  });

  get actionsArray(): FormArray {
    return this.macroForm.get('actions') as FormArray;
  }

  ngOnInit(): void {
    this.store.dispatch(MacrosActions.loadMacros());

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam && idParam !== 'new') {
      this.isEditing = true;
      this.editingId = Number(idParam);
      this.macroEntities$.pipe(first((entities) => !!entities[this.editingId!])).subscribe((entities) => {
        const macro = entities[this.editingId!];
        if (macro) {
          this.macroForm.patchValue({
            name: macro.name,
            visibility: macro.visibility,
          });
          // Parse actions
          if (macro.actions) {
            try {
              const actions = JSON.parse(macro.actions);
              if (Array.isArray(actions)) {
                actions.forEach((a: { type: string; params: string }) => {
                  this.actionsArray.push(
                    this.fb.group({
                      type: [a.type || '', Validators.required],
                      params: [a.params || ''],
                    })
                  );
                });
              }
            } catch {
              // ignore parse errors
            }
          }
        }
      });
    }
  }

  addAction(): void {
    this.actionsArray.push(
      this.fb.group({
        type: ['', Validators.required],
        params: [''],
      })
    );
  }

  removeAction(index: number): void {
    this.actionsArray.removeAt(index);
  }

  moveAction(fromIndex: number, toIndex: number): void {
    if (toIndex < 0 || toIndex >= this.actionsArray.length) return;
    const control = this.actionsArray.at(fromIndex);
    this.actionsArray.removeAt(fromIndex);
    this.actionsArray.insert(toIndex, control);
  }

  getParamLabel(type: string): string {
    if (!type) return '';
    return ACTION_PARAM_LABELS[type] || '';
  }

  onSubmit(): void {
    if (this.macroForm.invalid) return;

    const formValue = this.macroForm.value;
    const data = {
      name: formValue.name,
      visibility: formValue.visibility,
      actions: JSON.stringify(formValue.actions),
    };

    if (this.isEditing && this.editingId) {
      this.store.dispatch(MacrosActions.updateMacro({ id: this.editingId, data }));
    } else {
      this.store.dispatch(MacrosActions.createMacro({ data }));
    }
    this.goBack();
  }

  goBack(): void {
    this.router.navigate(['/settings/macros']);
  }
}
