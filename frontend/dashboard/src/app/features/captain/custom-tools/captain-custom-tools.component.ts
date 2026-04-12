import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { BehaviorSubject } from 'rxjs';

interface CustomTool {
  id: number;
  assistantId: number;
  name: string;
  description: string | null;
  endpointUrl: string;
  parameters: string;
  createdAt: string;
  updatedAt: string;
}

@Component({
  selector: 'app-captain-custom-tools',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <div>
          <h2 class="text-lg font-semibold text-gray-900">Custom Tools</h2>
          <p class="text-sm text-gray-500 mt-0.5">Register external API tools this assistant can call during conversations.</p>
        </div>
        <button
          (click)="showForm = true; editingTool = null; resetForm()"
          class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
        >
          Register Tool
        </button>
      </div>

      @if (showForm) {
        <div class="bg-white rounded-lg border border-gray-200 p-5 mb-6">
          <h3 class="text-sm font-semibold text-gray-900 mb-4">{{ editingTool ? 'Edit Tool' : 'Register Tool' }}</h3>
          <form [formGroup]="toolForm" (ngSubmit)="saveTool()" class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Tool Name <span class="text-red-500">*</span></label>
              <input
                formControlName="name"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
                placeholder="e.g. lookup_order"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
              <input
                formControlName="description"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
                placeholder="What does this tool do?"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Endpoint URL <span class="text-red-500">*</span></label>
              <input
                formControlName="endpointUrl"
                type="url"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm font-mono focus:border-blue-500 focus:outline-none"
                placeholder="https://api.example.com/tool"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">
                Parameters Schema
                <span class="ml-1 text-xs text-gray-400 font-normal">JSON Schema object</span>
              </label>
              <textarea
                formControlName="parameters"
                rows="4"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm font-mono focus:border-blue-500 focus:outline-none"
                placeholder='{"orderId": {"type": "string", "description": "The order ID"}}'
              ></textarea>
            </div>
            <div class="flex gap-3">
              <button
                type="submit"
                [disabled]="toolForm.invalid || saving"
                class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 rounded-lg"
              >{{ saving ? 'Saving...' : 'Save' }}</button>
              <button type="button" (click)="showForm = false" class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 hover:bg-gray-50 rounded-lg">Cancel</button>
            </div>
          </form>
        </div>
      }

      @if (loading$ | async) {
        <div class="flex justify-center py-10">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else if ((tools$ | async); as tools) {
        @if (tools.length === 0 && !showForm) {
          <div class="text-center py-12 bg-white rounded-lg border border-gray-200">
            <p class="text-sm text-gray-500">No tools registered. Add external APIs for the assistant to call.</p>
          </div>
        } @else {
          <div class="space-y-3">
            @for (tool of tools; track tool.id) {
              <div class="bg-white rounded-lg border border-gray-200 p-4">
                <div class="flex items-start justify-between gap-4">
                  <div class="flex-1 min-w-0">
                    <p class="text-sm font-medium text-gray-900 font-mono">{{ tool.name }}</p>
                    @if (tool.description) {
                      <p class="text-xs text-gray-500 mt-0.5">{{ tool.description }}</p>
                    }
                    <p class="text-xs text-gray-400 mt-1 truncate">{{ tool.endpointUrl }}</p>
                  </div>
                  <div class="flex items-center gap-2 shrink-0">
                    <button (click)="editTool(tool)" class="text-sm text-blue-600 hover:text-blue-700">Edit</button>
                    <button (click)="deleteTool(tool.id)" class="text-sm text-red-600 hover:text-red-700">Delete</button>
                  </div>
                </div>
              </div>
            }
          </div>
        }
      }
    </div>
  `,
  styles: [':host { display: block; height: 100%; overflow-y: auto; }'],
})
export class CaptainCustomToolsComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);

  tools$ = new BehaviorSubject<CustomTool[]>([]);
  loading$ = new BehaviorSubject<boolean>(false);
  showForm = false;
  saving = false;
  editingTool: CustomTool | null = null;

  private assistantId!: number;

  toolForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    endpointUrl: ['', [Validators.required, Validators.pattern(/^https?:\/\/.+/)]],
    parameters: ['{}'],
  });

  private get basePath(): string {
    const accountId = this.auth.currentAccountId();
    return `/accounts/${accountId}/captain/assistants/${this.assistantId}/tools`;
  }

  ngOnInit(): void {
    this.assistantId = +this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  private load(): void {
    this.loading$.next(true);
    this.api.get<CustomTool[]>(this.basePath).subscribe({
      next: (data) => { this.tools$.next(data); this.loading$.next(false); },
      error: () => this.loading$.next(false),
    });
  }

  editTool(t: CustomTool): void {
    this.editingTool = t;
    this.showForm = true;
    this.toolForm.patchValue({ name: t.name, description: t.description ?? '', endpointUrl: t.endpointUrl, parameters: t.parameters });
  }

  resetForm(): void {
    this.toolForm.reset({ name: '', description: '', endpointUrl: '', parameters: '{}' });
  }

  saveTool(): void {
    if (this.toolForm.invalid) return;
    this.saving = true;
    const payload = this.toolForm.value;

    const req$ = this.editingTool
      ? this.api.put(`${this.basePath}/${this.editingTool.id}`, payload)
      : this.api.post(this.basePath, payload);

    req$.subscribe({
      next: () => {
        this.saving = false;
        this.showForm = false;
        this.editingTool = null;
        this.load();
      },
      error: () => { this.saving = false; },
    });
  }

  deleteTool(id: number): void {
    if (!confirm('Unregister this tool?')) return;
    this.api.delete(`${this.basePath}/${id}`).subscribe({ next: () => this.load() });
  }
}
