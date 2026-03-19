import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { CannedResponsesActions } from '@store/canned-responses/canned-responses.actions';
import { selectAllCannedResponses, selectCannedResponsesLoading } from '@store/canned-responses/canned-responses.selectors';
import { CannedResponse } from '@core/models/canned-response.model';

@Component({
  selector: 'app-canned-response-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-lg font-semibold text-gray-900">Canned Responses</h2>
      </div>

      @if (loading$ | async) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if ((cannedResponses$ | async); as responses) {
          <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
            @if (responses.length > 0) {
              <ul class="divide-y divide-gray-200">
                @for (response of responses; track response.id) {
                  <li class="px-6 py-4 hover:bg-gray-50 transition-colors">
                    @if (editingId === response.id) {
                      <form [formGroup]="editForm" (ngSubmit)="saveEdit(response.id)" class="space-y-3">
                        <div class="flex items-center gap-4">
                          <div class="w-40">
                            <input
                              formControlName="shortCode"
                              class="w-full rounded-lg border border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                              placeholder="Short code"
                            />
                          </div>
                          <div class="flex-1">
                            <textarea
                              formControlName="content"
                              rows="2"
                              class="w-full rounded-lg border border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                              placeholder="Response content"
                            ></textarea>
                          </div>
                        </div>
                        <div class="flex justify-end gap-2">
                          <button
                            type="button"
                            (click)="cancelEdit()"
                            class="px-3 py-1.5 text-xs font-medium text-gray-600 border border-gray-300 rounded-lg hover:bg-gray-50"
                          >
                            Cancel
                          </button>
                          <button
                            type="submit"
                            [disabled]="editForm.invalid"
                            class="px-3 py-1.5 text-xs font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50"
                          >
                            Save
                          </button>
                        </div>
                      </form>
                    } @else {
                      <div class="flex items-center justify-between">
                        <div class="flex-1">
                          <div class="flex items-center gap-3">
                            <code class="px-2 py-0.5 bg-gray-100 rounded text-xs font-mono text-blue-700">{{ response.shortCode }}</code>
                          </div>
                          <p class="text-sm text-gray-700 mt-1 line-clamp-2">{{ response.content }}</p>
                        </div>
                        <div class="flex items-center gap-3 ml-4">
                          <button (click)="startEdit(response)" class="text-sm text-blue-600 hover:text-blue-800">Edit</button>
                          <button (click)="deleteResponse(response.id)" class="text-sm text-red-600 hover:text-red-800">Delete</button>
                        </div>
                      </div>
                    }
                  </li>
                }
              </ul>
            } @else {
              <div class="text-center py-8">
                <p class="text-sm text-gray-500">No canned responses yet.</p>
              </div>
            }

            <!-- Inline Create Form -->
            <div class="border-t border-gray-200 px-6 py-4 bg-gray-50">
              <form [formGroup]="createForm" (ngSubmit)="createResponse()" class="flex items-start gap-4">
                <div class="w-40">
                  <input
                    formControlName="shortCode"
                    class="w-full rounded-lg border border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    placeholder="Short code"
                  />
                </div>
                <div class="flex-1">
                  <textarea
                    formControlName="content"
                    rows="2"
                    class="w-full rounded-lg border border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    placeholder="Response content"
                  ></textarea>
                </div>
                <button
                  type="submit"
                  [disabled]="createForm.invalid"
                  class="px-4 py-1.5 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Add
                </button>
              </form>
            </div>
          </div>
        }
      }
    </div>
  `,
  styles: [`:host { display: block; height: 100%; }`],
})
export class CannedResponseListComponent implements OnInit {
  private store = inject(Store);
  private fb = inject(FormBuilder);

  cannedResponses$ = this.store.select(selectAllCannedResponses);
  loading$ = this.store.select(selectCannedResponsesLoading);

  editingId: number | null = null;

  createForm: FormGroup = this.fb.group({
    shortCode: ['', Validators.required],
    content: ['', Validators.required],
  });

  editForm: FormGroup = this.fb.group({
    shortCode: ['', Validators.required],
    content: ['', Validators.required],
  });

  ngOnInit(): void {
    this.store.dispatch(CannedResponsesActions.loadCannedResponses());
  }

  createResponse(): void {
    if (this.createForm.invalid) return;
    this.store.dispatch(CannedResponsesActions.createCannedResponse({ data: this.createForm.value }));
    this.createForm.reset();
  }

  startEdit(response: CannedResponse): void {
    this.editingId = response.id;
    this.editForm.patchValue({ shortCode: response.shortCode, content: response.content });
  }

  cancelEdit(): void {
    this.editingId = null;
  }

  saveEdit(id: number): void {
    if (this.editForm.invalid) return;
    this.store.dispatch(CannedResponsesActions.updateCannedResponse({ id, data: this.editForm.value }));
    this.editingId = null;
  }

  deleteResponse(id: number): void {
    if (confirm('Are you sure you want to delete this canned response?')) {
      this.store.dispatch(CannedResponsesActions.deleteCannedResponse({ id }));
    }
  }
}
