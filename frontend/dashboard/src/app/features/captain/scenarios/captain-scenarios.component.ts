import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { CaptainService } from '@core/services/captain.service';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { BehaviorSubject } from 'rxjs';

interface Scenario {
  id: number;
  assistantId: number;
  title: string;
  description: string | null;
  steps: string;
  createdAt: string;
  updatedAt: string;
}

@Component({
  selector: 'app-captain-scenarios',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <div>
          <h2 class="text-lg font-semibold text-gray-900">Scenarios</h2>
          <p class="text-sm text-gray-500 mt-0.5">Define conversation flows and response scenarios for this assistant.</p>
        </div>
        <button
          (click)="showForm = true; editingScenario = null; resetForm()"
          class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
        >
          New Scenario
        </button>
      </div>

      <!-- Scenario Form -->
      @if (showForm) {
        <div class="bg-white rounded-lg border border-gray-200 p-5 mb-6">
          <h3 class="text-sm font-semibold text-gray-900 mb-4">{{ editingScenario ? 'Edit Scenario' : 'New Scenario' }}</h3>
          <form [formGroup]="scenarioForm" (ngSubmit)="saveScenario()" class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Title <span class="text-red-500">*</span></label>
              <input
                formControlName="title"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
                placeholder="e.g. Handle billing questions"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
              <textarea
                formControlName="description"
                rows="2"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
                placeholder="When this scenario should trigger..."
              ></textarea>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">
                Steps
                <span class="ml-1 text-xs text-gray-400 font-normal">JSON array of step objects</span>
              </label>
              <textarea
                formControlName="steps"
                rows="5"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm font-mono focus:border-blue-500 focus:outline-none"
                placeholder='[{"action": "reply", "content": "How can I help with billing?"}]'
              ></textarea>
            </div>
            <div class="flex gap-3">
              <button
                type="submit"
                [disabled]="scenarioForm.invalid || saving"
                class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 rounded-lg"
              >{{ saving ? 'Saving...' : 'Save' }}</button>
              <button type="button" (click)="showForm = false" class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 hover:bg-gray-50 rounded-lg">Cancel</button>
            </div>
          </form>
        </div>
      }

      <!-- Scenarios List -->
      @if (loading$ | async) {
        <div class="flex justify-center py-10">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if (scenarios$ | async; as scenarios) {
          @if (scenarios.length === 0 && !showForm) {
            <div class="text-center py-12 bg-white rounded-lg border border-gray-200">
              <p class="text-sm text-gray-500">No scenarios yet. Create one to define conversation flows.</p>
            </div>
          } @else {
            <div class="space-y-3">
              @for (s of scenarios; track s.id) {
                <div class="bg-white rounded-lg border border-gray-200 p-4">
                  <div class="flex items-start justify-between gap-4">
                    <div class="flex-1 min-w-0">
                      <p class="text-sm font-medium text-gray-900">{{ s.title }}</p>
                      @if (s.description) {
                        <p class="text-xs text-gray-500 mt-0.5">{{ s.description }}</p>
                      }
                    </div>
                    <div class="flex items-center gap-2 shrink-0">
                      <button (click)="editScenario(s)" class="text-sm text-blue-600 hover:text-blue-700">Edit</button>
                      <button (click)="deleteScenario(s.id)" class="text-sm text-red-600 hover:text-red-700">Delete</button>
                    </div>
                  </div>
                </div>
              }
            </div>
          }
        }
      }
    </div>
  `,
  styles: [':host { display: block; height: 100%; overflow-y: auto; }'],
})
export class CaptainScenariosComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);

  scenarios$ = new BehaviorSubject<Scenario[]>([]);
  loading$ = new BehaviorSubject<boolean>(false);
  showForm = false;
  saving = false;
  editingScenario: Scenario | null = null;

  private assistantId!: number;

  scenarioForm: FormGroup = this.fb.group({
    title: ['', Validators.required],
    description: [''],
    steps: ['[]'],
  });

  private get basePath(): string {
    const accountId = this.auth.currentAccountId();
    return `/accounts/${accountId}/captain/assistants/${this.assistantId}/scenarios`;
  }

  ngOnInit(): void {
    this.assistantId = +this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  private load(): void {
    this.loading$.next(true);
    this.api.get<Scenario[]>(this.basePath).subscribe({
      next: (data) => { this.scenarios$.next(data); this.loading$.next(false); },
      error: () => this.loading$.next(false),
    });
  }

  editScenario(s: Scenario): void {
    this.editingScenario = s;
    this.showForm = true;
    this.scenarioForm.patchValue({ title: s.title, description: s.description ?? '', steps: s.steps });
  }

  resetForm(): void {
    this.scenarioForm.reset({ title: '', description: '', steps: '[]' });
  }

  saveScenario(): void {
    if (this.scenarioForm.invalid) return;
    this.saving = true;
    const payload = this.scenarioForm.value;

    const req$ = this.editingScenario
      ? this.api.put(`${this.basePath}/${this.editingScenario.id}`, payload)
      : this.api.post(this.basePath, payload);

    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showForm = false;
        this.editingScenario = null;
        this.load();
      },
      error: () => { this.saving = false; },
    });
  }

  deleteScenario(id: number): void {
    if (!confirm('Delete this scenario?')) return;
    this.api.delete(`${this.basePath}/${id}`).subscribe({ next: () => this.load() });
  }
}
