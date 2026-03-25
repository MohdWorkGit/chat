import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { LabelsActions } from '@store/labels/labels.actions';
import { selectAllLabels, selectLabelsLoading } from '@store/labels/labels.selectors';
import { Label } from '@core/models/label.model';

@Component({
  selector: 'app-label-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-lg font-semibold text-gray-900">Labels</h2>
      </div>

      <!-- Loading State -->
      @if (loading$ | async) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if ((labels$ | async); as labels) {
          <!-- Labels List -->
          <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
            @if (labels.length > 0) {
              <ul class="divide-y divide-gray-200">
                @for (label of labels; track label.id) {
                  <li class="px-6 py-4 hover:bg-gray-50 transition-colors">
                    @if (editingLabelId === label.id) {
                      <!-- Inline Edit Form -->
                      <form [formGroup]="editForm" (ngSubmit)="saveEdit(label.id)" class="flex items-center gap-4">
                        <input
                          type="color"
                          formControlName="color"
                          class="h-8 w-8 rounded border border-gray-300 cursor-pointer"
                        />
                        <input
                          formControlName="title"
                          class="flex-1 rounded-lg border border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                          placeholder="Label title"
                        />
                        <input
                          formControlName="description"
                          class="flex-1 rounded-lg border border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                          placeholder="Description (optional)"
                        />
                        <div class="flex items-center gap-2">
                          <button
                            type="submit"
                            [disabled]="editForm.invalid"
                            class="px-3 py-1.5 text-xs font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 transition-colors"
                          >
                            Save
                          </button>
                          <button
                            type="button"
                            (click)="cancelEdit()"
                            class="px-3 py-1.5 text-xs font-medium text-gray-600 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                          >
                            Cancel
                          </button>
                        </div>
                      </form>
                    } @else {
                      <!-- Label Display -->
                      <div class="flex items-center justify-between">
                        <div class="flex items-center gap-3">
                          <span
                            class="h-4 w-4 rounded-full flex-shrink-0"
                            [style.background-color]="label.color"
                          ></span>
                          <div>
                            <span class="text-sm font-medium text-gray-900">{{ label.title }}</span>
                            @if (label.description) {
                              <p class="text-xs text-gray-500">{{ label.description }}</p>
                            }
                          </div>
                        </div>
                        <div class="flex items-center gap-3">
                          <button
                            (click)="startEdit(label)"
                            class="text-sm text-blue-600 hover:text-blue-800"
                          >
                            Edit
                          </button>
                          <button
                            (click)="deleteLabel(label.id)"
                            class="text-sm text-red-600 hover:text-red-800"
                          >
                            Delete
                          </button>
                        </div>
                      </div>
                    }
                  </li>
                }
              </ul>
            } @else {
              <div class="text-center py-8">
                <p class="text-sm text-gray-500">No labels created yet.</p>
              </div>
            }

            <!-- Inline Create Form -->
            <div class="border-t border-gray-200 px-6 py-4 bg-gray-50">
              <form [formGroup]="createForm" (ngSubmit)="createLabel()" class="flex items-center gap-4">
                <input
                  type="color"
                  formControlName="color"
                  class="h-8 w-8 rounded border border-gray-300 cursor-pointer"
                />
                <input
                  formControlName="title"
                  class="flex-1 rounded-lg border border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="Label title"
                />
                <input
                  formControlName="description"
                  class="flex-1 rounded-lg border border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="Description (optional)"
                />
                <button
                  type="submit"
                  [disabled]="createForm.invalid"
                  class="px-4 py-1.5 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                >
                  Add Label
                </button>
              </form>
            </div>
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
export class LabelListComponent implements OnInit {
  private store = inject(Store);
  private fb = inject(FormBuilder);

  labels$ = this.store.select(selectAllLabels);
  loading$ = this.store.select(selectLabelsLoading);

  editingLabelId: number | null = null;

  createForm: FormGroup = this.fb.group({
    title: ['', Validators.required],
    description: [''],
    color: ['#1F93FF'],
  });

  editForm: FormGroup = this.fb.group({
    title: ['', Validators.required],
    description: [''],
    color: ['#1F93FF'],
  });

  ngOnInit(): void {
    this.store.dispatch(LabelsActions.loadLabels());
  }

  createLabel(): void {
    if (this.createForm.invalid) return;
    this.store.dispatch(LabelsActions.createLabel({ data: this.createForm.value }));
    this.createForm.reset({ color: '#1F93FF', title: '', description: '' });
  }

  startEdit(label: Label): void {
    this.editingLabelId = label.id;
    this.editForm.patchValue({
      title: label.title,
      description: label.description || '',
      color: label.color,
    });
  }

  cancelEdit(): void {
    this.editingLabelId = null;
  }

  saveEdit(labelId: number): void {
    if (this.editForm.invalid) return;
    this.store.dispatch(
      LabelsActions.updateLabel({
        id: labelId,
        data: this.editForm.value,
      })
    );
    this.editingLabelId = null;
  }

  deleteLabel(labelId: number): void {
    if (confirm('Are you sure you want to delete this label?')) {
      this.store.dispatch(LabelsActions.deleteLabel({ id: labelId }));
    }
  }
}
