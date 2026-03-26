import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { AutomationsActions } from '@store/automations/automations.actions';

@Component({
  selector: 'app-automation-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="h-full overflow-y-auto bg-gray-50">
      <div class="max-w-3xl mx-auto px-6 py-6">
        <!-- Header -->
        <div class="flex items-center justify-between mb-6">
          <div>
            <a routerLink="/settings/automations" class="text-sm text-blue-600 hover:text-blue-500 flex items-center gap-1 mb-2">
              <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5 8.25 12l7.5-7.5" />
              </svg>
              Back to Automations
            </a>
            <h1 class="text-xl font-semibold text-gray-900">New Automation Rule</h1>
          </div>
        </div>

        <!-- Form -->
        <form [formGroup]="automationForm" (ngSubmit)="onSubmit()" class="space-y-6">
          <!-- Basic Info -->
          <div class="bg-white rounded-lg border border-gray-200 p-6">
            <h3 class="text-sm font-semibold text-gray-900 mb-4">Basic Information</h3>
            <div class="space-y-4">
              <div>
                <label class="block text-sm font-medium text-gray-700">Name <span class="text-red-500">*</span></label>
                <input
                  formControlName="name"
                  class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="Automation rule name"
                />
                @if (automationForm.get('name')?.hasError('required') && automationForm.get('name')?.touched) {
                  <p class="mt-1 text-xs text-red-500">Name is required</p>
                }
              </div>

              <div>
                <label class="block text-sm font-medium text-gray-700">Description</label>
                <textarea
                  formControlName="description"
                  rows="2"
                  class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="Describe what this automation does"
                ></textarea>
              </div>

              <div>
                <label class="block text-sm font-medium text-gray-700">Event Type <span class="text-red-500">*</span></label>
                <select
                  formControlName="eventName"
                  class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option value="conversation_created">Conversation Created</option>
                  <option value="conversation_updated">Conversation Updated</option>
                  <option value="message_created">Message Created</option>
                  <option value="conversation_status_changed">Status Changed</option>
                </select>
              </div>
            </div>
          </div>

          <!-- Conditions -->
          <div class="bg-white rounded-lg border border-gray-200 p-6">
            <div class="flex items-center justify-between mb-4">
              <h3 class="text-sm font-semibold text-gray-900">Conditions</h3>
              <button
                type="button"
                (click)="addCondition()"
                class="inline-flex items-center gap-1 px-3 py-1.5 text-xs font-medium text-blue-600 border border-blue-300 rounded-lg hover:bg-blue-50 transition-colors"
              >
                <svg class="h-3.5 w-3.5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
                </svg>
                Add Condition
              </button>
            </div>

            @if (conditions.length === 0) {
              <p class="text-sm text-gray-400 text-center py-4">No conditions added. The automation will trigger for all events of this type.</p>
            }

            <div formArrayName="conditions">
              @for (condition of conditions.controls; track $index; let i = $index) {
                <div class="flex items-start gap-3 mb-3" [formGroupName]="i">
                  <div class="flex-1">
                    <label class="block text-xs font-medium text-gray-500 mb-1">Field</label>
                    <input
                      formControlName="attributeKey"
                      class="w-full rounded-lg border border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                      placeholder="e.g. status, priority"
                    />
                  </div>
                  <div class="flex-1">
                    <label class="block text-xs font-medium text-gray-500 mb-1">Operator</label>
                    <select
                      formControlName="filterOperator"
                      class="w-full rounded-lg border border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    >
                      <option value="equal_to">Equal to</option>
                      <option value="not_equal_to">Not equal to</option>
                      <option value="contains">Contains</option>
                      <option value="does_not_contain">Does not contain</option>
                    </select>
                  </div>
                  <div class="flex-1">
                    <label class="block text-xs font-medium text-gray-500 mb-1">Value</label>
                    <input
                      formControlName="values"
                      class="w-full rounded-lg border border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                      placeholder="Value"
                    />
                  </div>
                  <div class="pt-5">
                    <button
                      type="button"
                      (click)="removeCondition(i)"
                      class="p-1.5 text-red-400 hover:text-red-600 hover:bg-red-50 rounded transition-colors"
                    >
                      <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="m14.74 9-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 0 1-2.244 2.077H8.084a2.25 2.25 0 0 1-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 0 0-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 0 1 3.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 0 0-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 0 0-7.5 0" />
                      </svg>
                    </button>
                  </div>
                </div>
              }
            </div>
          </div>

          <!-- Actions -->
          <div class="bg-white rounded-lg border border-gray-200 p-6">
            <div class="flex items-center justify-between mb-4">
              <h3 class="text-sm font-semibold text-gray-900">Actions</h3>
              <button
                type="button"
                (click)="addAction()"
                class="inline-flex items-center gap-1 px-3 py-1.5 text-xs font-medium text-blue-600 border border-blue-300 rounded-lg hover:bg-blue-50 transition-colors"
              >
                <svg class="h-3.5 w-3.5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
                </svg>
                Add Action
              </button>
            </div>

            @if (actions.length === 0) {
              <p class="text-sm text-gray-400 text-center py-4">No actions added. Add at least one action for the automation to perform.</p>
            }

            <div formArrayName="actions">
              @for (action of actions.controls; track $index; let i = $index) {
                <div class="flex items-start gap-3 mb-3" [formGroupName]="i">
                  <div class="flex-1">
                    <label class="block text-xs font-medium text-gray-500 mb-1">Action Type</label>
                    <select
                      formControlName="actionName"
                      class="w-full rounded-lg border border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    >
                      <option value="assign_agent">Assign Agent</option>
                      <option value="assign_team">Assign Team</option>
                      <option value="add_label">Add Label</option>
                      <option value="remove_label">Remove Label</option>
                      <option value="send_email">Send Email</option>
                      <option value="send_message">Send Message</option>
                      <option value="change_status">Change Status</option>
                      <option value="change_priority">Change Priority</option>
                      <option value="mute_conversation">Mute Conversation</option>
                    </select>
                  </div>
                  <div class="flex-1">
                    <label class="block text-xs font-medium text-gray-500 mb-1">Parameters</label>
                    <input
                      formControlName="actionParams"
                      class="w-full rounded-lg border border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                      placeholder="Comma-separated values"
                    />
                  </div>
                  <div class="pt-5">
                    <button
                      type="button"
                      (click)="removeAction(i)"
                      class="p-1.5 text-red-400 hover:text-red-600 hover:bg-red-50 rounded transition-colors"
                    >
                      <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="m14.74 9-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 0 1-2.244 2.077H8.084a2.25 2.25 0 0 1-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 0 0-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 0 1 3.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 0 0-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 0 0-7.5 0" />
                      </svg>
                    </button>
                  </div>
                </div>
              }
            </div>
          </div>

          <!-- Submit -->
          <div class="flex items-center gap-3">
            <button
              type="submit"
              [disabled]="automationForm.invalid"
              class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              Create Automation
            </button>
            <button
              type="button"
              (click)="onCancel()"
              class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
    }
  `],
})
export class AutomationFormComponent {
  private store = inject(Store);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  automationForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    eventName: ['conversation_created', Validators.required],
    conditions: this.fb.array([]),
    actions: this.fb.array([]),
  });

  get conditions(): FormArray {
    return this.automationForm.get('conditions') as FormArray;
  }

  get actions(): FormArray {
    return this.automationForm.get('actions') as FormArray;
  }

  addCondition(): void {
    this.conditions.push(
      this.fb.group({
        attributeKey: ['', Validators.required],
        filterOperator: ['equal_to'],
        values: [''],
      })
    );
  }

  removeCondition(index: number): void {
    this.conditions.removeAt(index);
  }

  addAction(): void {
    this.actions.push(
      this.fb.group({
        actionName: ['assign_agent', Validators.required],
        actionParams: [''],
      })
    );
  }

  removeAction(index: number): void {
    this.actions.removeAt(index);
  }

  onSubmit(): void {
    if (this.automationForm.invalid) return;

    const formValue = this.automationForm.value;

    const conditions = formValue.conditions.map((c: { attributeKey: string; filterOperator: string; values: string }) => ({
      attributeKey: c.attributeKey,
      filterOperator: c.filterOperator,
      values: c.values ? c.values.split(',').map((v: string) => v.trim()) : [],
    }));

    const actions = formValue.actions.map((a: { actionName: string; actionParams: string }) => ({
      actionName: a.actionName,
      actionParams: a.actionParams ? a.actionParams.split(',').map((v: string) => v.trim()) : [],
    }));

    this.store.dispatch(
      AutomationsActions.createAutomation({
        data: {
          name: formValue.name,
          description: formValue.description || undefined,
          eventName: formValue.eventName,
          conditions,
          conditionOperator: 'AND',
          actions,
          active: true,
        },
      })
    );
    this.router.navigate(['/settings/automations']);
  }

  onCancel(): void {
    this.router.navigate(['/settings/automations']);
  }
}
