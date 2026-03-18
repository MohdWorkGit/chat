import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { AutomationsActions } from '@store/automations/automations.actions';
import { selectAllAutomations, selectAutomationsLoading } from '@store/automations/automations.selectors';
import { AutomationRule } from '@core/models/automation.model';

@Component({
  selector: 'app-automation-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-lg font-semibold text-gray-900">Automation Rules</h2>
        <button
          (click)="showCreateForm = !showCreateForm"
          class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
        >
          {{ showCreateForm ? 'Cancel' : 'New Automation' }}
        </button>
      </div>

      @if (showCreateForm) {
        <div class="bg-white rounded-lg border border-gray-200 p-6 mb-6">
          <h3 class="text-sm font-semibold text-gray-900 mb-4">Create Automation Rule</h3>
          <form [formGroup]="createForm" (ngSubmit)="createAutomation()">
            <div class="grid grid-cols-2 gap-4 mb-4">
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">Name</label>
                <input
                  formControlName="name"
                  class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="Automation rule name"
                />
              </div>
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">Event</label>
                <select
                  formControlName="eventName"
                  class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option value="conversation_created">Conversation Created</option>
                  <option value="conversation_updated">Conversation Updated</option>
                  <option value="message_created">Message Created</option>
                  <option value="conversation_status_changed">Status Changed</option>
                </select>
              </div>
            </div>
            <div class="mb-4">
              <label class="block text-xs font-medium text-gray-700 mb-1">Description</label>
              <textarea
                formControlName="description"
                rows="2"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="Describe what this automation does"
              ></textarea>
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
        @if ((automations$ | async); as automations) {
          <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
            @if (automations.length > 0) {
              <ul class="divide-y divide-gray-200">
                @for (rule of automations; track rule.id) {
                  <li class="px-6 py-4 hover:bg-gray-50 transition-colors">
                    <div class="flex items-center justify-between">
                      <div class="flex-1">
                        <div class="flex items-center gap-3">
                          <span class="text-sm font-medium text-gray-900">{{ rule.name }}</span>
                          <span
                            class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium"
                            [class]="rule.active ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-600'"
                          >
                            {{ rule.active ? 'Active' : 'Inactive' }}
                          </span>
                        </div>
                        @if (rule.description) {
                          <p class="text-xs text-gray-500 mt-1">{{ rule.description }}</p>
                        }
                        <div class="flex items-center gap-4 mt-2">
                          <span class="text-xs text-gray-400">
                            Event: <span class="text-gray-600">{{ formatEvent(rule.eventName) }}</span>
                          </span>
                          <span class="text-xs text-gray-400">
                            Conditions: <span class="text-gray-600">{{ rule.conditions?.length || 0 }}</span>
                          </span>
                          <span class="text-xs text-gray-400">
                            Actions: <span class="text-gray-600">{{ rule.actions?.length || 0 }}</span>
                          </span>
                        </div>
                      </div>
                      <div class="flex items-center gap-3">
                        <button
                          (click)="toggleActive(rule)"
                          class="text-sm"
                          [class]="rule.active ? 'text-yellow-600 hover:text-yellow-800' : 'text-green-600 hover:text-green-800'"
                        >
                          {{ rule.active ? 'Disable' : 'Enable' }}
                        </button>
                        <button
                          (click)="deleteAutomation(rule.id)"
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
                <p class="text-sm text-gray-500">No automation rules created yet.</p>
                <p class="text-xs text-gray-400 mt-1">Create your first rule to automate workflows.</p>
              </div>
            }
          </div>
        }
      }
    </div>
  `,
  styles: [`:host { display: block; height: 100%; }`],
})
export class AutomationListComponent implements OnInit {
  private store = inject(Store);
  private fb = inject(FormBuilder);

  automations$ = this.store.select(selectAllAutomations);
  loading$ = this.store.select(selectAutomationsLoading);

  showCreateForm = false;

  createForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    eventName: ['conversation_created', Validators.required],
    description: [''],
  });

  ngOnInit(): void {
    this.store.dispatch(AutomationsActions.loadAutomations());
  }

  createAutomation(): void {
    if (this.createForm.invalid) return;
    this.store.dispatch(
      AutomationsActions.createAutomation({
        data: {
          ...this.createForm.value,
          conditions: [],
          conditionOperator: 'AND',
          actions: [],
          active: true,
        },
      })
    );
    this.createForm.reset({ eventName: 'conversation_created' });
    this.showCreateForm = false;
  }

  toggleActive(rule: AutomationRule): void {
    this.store.dispatch(
      AutomationsActions.updateAutomation({
        id: rule.id,
        data: { active: !rule.active },
      })
    );
  }

  deleteAutomation(id: number): void {
    if (confirm('Are you sure you want to delete this automation rule?')) {
      this.store.dispatch(AutomationsActions.deleteAutomation({ id }));
    }
  }

  formatEvent(event: string): string {
    return event.replace(/_/g, ' ').replace(/\b\w/g, (c) => c.toUpperCase());
  }
}
