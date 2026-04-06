import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CaptainService, CaptainAssistant } from '@core/services/captain.service';
import { AuthService } from '@core/services/auth.service';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-assistant-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-lg font-semibold text-gray-900">Captain AI Assistants</h2>
        <button
          (click)="openCreateModal()"
          class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
        >
          New Assistant
        </button>
      </div>

      <!-- Loading State -->
      @if (loading) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if (assistants.length > 0) {
          <!-- Assistant Cards -->
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            @for (assistant of assistants; track assistant.id) {
              <div class="bg-white rounded-lg border border-gray-200 p-5 hover:shadow-md transition-shadow">
                <div class="flex items-start justify-between mb-3">
                  <h3 class="text-sm font-semibold text-gray-900 truncate">{{ assistant.name }}</h3>
                  <span class="ml-2 inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-blue-100 text-blue-800">
                    {{ assistant.model }}
                  </span>
                </div>
                <p class="text-xs text-gray-500 mb-3 line-clamp-2">{{ assistant.description || 'No description' }}</p>
                <div class="flex items-center text-xs text-gray-400 mb-4">
                  <span>Temperature: {{ assistant.temperature }}</span>
                </div>
                <div class="flex items-center justify-between border-t border-gray-100 pt-3">
                  <a
                    [routerLink]="['/captain', assistant.id, 'documents']"
                    class="text-xs text-blue-600 hover:text-blue-800"
                  >
                    Documents
                  </a>
                  <div class="flex items-center gap-3">
                    <button
                      (click)="openEditModal(assistant)"
                      class="text-xs text-blue-600 hover:text-blue-800"
                    >
                      Edit
                    </button>
                    <button
                      (click)="deleteAssistant(assistant)"
                      class="text-xs text-red-600 hover:text-red-800"
                    >
                      Delete
                    </button>
                  </div>
                </div>
              </div>
            }
          </div>
        } @else {
          <!-- Empty State -->
          <div class="bg-white rounded-lg border border-gray-200 text-center py-12">
            <div class="mx-auto h-12 w-12 text-gray-400 mb-4">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M9.813 15.904L9 18.75l-.813-2.846a4.5 4.5 0 00-3.09-3.09L2.25 12l2.846-.813a4.5 4.5 0 003.09-3.09L9 5.25l.813 2.846a4.5 4.5 0 003.09 3.09L15.75 12l-2.846.813a4.5 4.5 0 00-3.09 3.09zM18.259 8.715L18 9.75l-.259-1.035a3.375 3.375 0 00-2.455-2.456L14.25 6l1.036-.259a3.375 3.375 0 002.455-2.456L18 2.25l.259 1.035a3.375 3.375 0 002.455 2.456L21.75 6l-1.036.259a3.375 3.375 0 00-2.455 2.456zM16.894 20.567L16.5 21.75l-.394-1.183a2.25 2.25 0 00-1.423-1.423L13.5 18.75l1.183-.394a2.25 2.25 0 001.423-1.423l.394-1.183.394 1.183a2.25 2.25 0 001.423 1.423l1.183.394-1.183.394a2.25 2.25 0 00-1.423 1.423z" />
              </svg>
            </div>
            <h3 class="text-sm font-medium text-gray-900 mb-1">No assistants yet</h3>
            <p class="text-xs text-gray-500 mb-4">Create your first Captain AI assistant to get started.</p>
            <button
              (click)="openCreateModal()"
              class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
            >
              New Assistant
            </button>
          </div>
        }
      }

      <!-- Modal -->
      @if (showModal) {
        <div class="fixed inset-0 z-50 overflow-y-auto">
          <div class="flex min-h-full items-center justify-center p-4">
            <div class="fixed inset-0 bg-black/30" (click)="closeModal()"></div>
            <div class="relative bg-white rounded-xl shadow-xl w-full max-w-lg p-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-4">
                {{ editingAssistant ? 'Edit Assistant' : 'New Assistant' }}
              </h3>
              <form [formGroup]="assistantForm" (ngSubmit)="saveAssistant()" class="space-y-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Name</label>
                  <input
                    formControlName="name"
                    class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    placeholder="Assistant name"
                  />
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
                  <input
                    formControlName="description"
                    class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    placeholder="Brief description"
                  />
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Model</label>
                  <select
                    formControlName="model"
                    class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 bg-white"
                  >
                    <option value="gpt-4">GPT-4</option>
                    <option value="gpt-4o">GPT-4o</option>
                    <option value="gpt-3.5-turbo">GPT-3.5 Turbo</option>
                    <option value="claude-3-opus">Claude 3 Opus</option>
                    <option value="claude-3-sonnet">Claude 3 Sonnet</option>
                  </select>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">
                    Temperature: {{ assistantForm.get('temperature')?.value }}
                  </label>
                  <input
                    type="range"
                    formControlName="temperature"
                    min="0"
                    max="1"
                    step="0.1"
                    class="w-full accent-blue-600"
                  />
                  <div class="flex justify-between text-xs text-gray-400">
                    <span>Precise</span>
                    <span>Creative</span>
                  </div>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Response Guidelines</label>
                  <textarea
                    formControlName="responseGuidelines"
                    rows="3"
                    class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    placeholder="Guidelines for how the assistant should respond..."
                  ></textarea>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Guardrails</label>
                  <textarea
                    formControlName="guardrails"
                    rows="3"
                    class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    placeholder="Guardrails and restrictions for the assistant..."
                  ></textarea>
                </div>
                <div class="flex justify-end gap-3 pt-2">
                  <button
                    type="button"
                    (click)="closeModal()"
                    class="px-4 py-2 text-sm font-medium text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    [disabled]="assistantForm.invalid"
                    class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                  >
                    {{ editingAssistant ? 'Update' : 'Create' }}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
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
export class AssistantListComponent implements OnInit {
  private captainService = inject(CaptainService);
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);

  assistants: CaptainAssistant[] = [];
  loading = true;
  showModal = false;
  editingAssistant: CaptainAssistant | null = null;

  private get accountId(): number {
    return this.auth.currentAccountId();
  }

  assistantForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    model: ['gpt-4', Validators.required],
    temperature: [0.7],
    responseGuidelines: [''],
    guardrails: [''],
  });

  ngOnInit(): void {
    this.loadAssistants();
  }

  loadAssistants(): void {
    this.loading = true;
    this.captainService.getAssistants(this.accountId).subscribe({
      next: (assistants) => {
        this.assistants = assistants;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  openCreateModal(): void {
    this.editingAssistant = null;
    this.assistantForm.reset({
      name: '',
      description: '',
      model: 'gpt-4',
      temperature: 0.7,
      responseGuidelines: '',
      guardrails: '',
    });
    this.showModal = true;
  }

  openEditModal(assistant: CaptainAssistant): void {
    this.editingAssistant = assistant;
    this.assistantForm.patchValue({
      name: assistant.name,
      description: assistant.description,
      model: assistant.model,
      temperature: assistant.temperature,
      responseGuidelines: assistant.responseGuidelines,
      guardrails: assistant.guardrails,
    });
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.editingAssistant = null;
  }

  saveAssistant(): void {
    if (this.assistantForm.invalid) return;

    const data = this.assistantForm.value;

    if (this.editingAssistant) {
      this.captainService
        .updateAssistant(this.accountId, this.editingAssistant.id, data)
        .subscribe({
          next: () => {
            this.closeModal();
            this.loadAssistants();
          },
        });
    } else {
      this.captainService.createAssistant(this.accountId, data).subscribe({
        next: () => {
          this.closeModal();
          this.loadAssistants();
        },
      });
    }
  }

  deleteAssistant(assistant: CaptainAssistant): void {
    if (confirm(`Are you sure you want to delete "${assistant.name}"?`)) {
      this.captainService
        .deleteAssistant(this.accountId, assistant.id)
        .subscribe({
          next: () => this.loadAssistants(),
        });
    }
  }
}
