import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { CustomAttributesActions } from '@store/custom-attributes/custom-attributes.actions';
import { selectCustomAttributeEntities, selectCustomAttributesLoading } from '@store/custom-attributes/custom-attributes.selectors';
import { first } from 'rxjs';

const ATTRIBUTE_TYPES = [
  { value: 'text', label: 'Text' },
  { value: 'number', label: 'Number' },
  { value: 'date', label: 'Date' },
  { value: 'list', label: 'List' },
  { value: 'checkbox', label: 'Checkbox' },
  { value: 'link', label: 'Link' },
  { value: 'currency', label: 'Currency' },
];

@Component({
  selector: 'app-custom-attribute-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="p-6">
      <!-- Header -->
      <div class="mb-6">
        <button
          (click)="goBack()"
          class="text-sm text-blue-600 hover:text-blue-800 mb-2 inline-flex items-center gap-1"
        >
          <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5L8.25 12l7.5-7.5" />
          </svg>
          Back to Custom Attributes
        </button>
        <h2 class="text-lg font-semibold text-gray-900">{{ isEditing ? 'Edit Custom Attribute' : 'New Custom Attribute' }}</h2>
      </div>

      <div class="bg-white rounded-lg border border-gray-200 p-6 max-w-2xl">
        <form [formGroup]="attrForm" (ngSubmit)="onSubmit()" class="space-y-5">
          <!-- Display Name -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Display Name</label>
            <input
              formControlName="displayName"
              (input)="onNameInput()"
              class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="e.g. Company Name"
            />
          </div>

          <!-- Key -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Key</label>
            <input
              formControlName="key"
              class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm font-mono focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="e.g. company_name"
            />
            <p class="text-xs text-gray-400 mt-1">Auto-generated from name. Used as the attribute identifier.</p>
          </div>

          <!-- Description -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
            <textarea
              formControlName="description"
              rows="2"
              class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="Brief description of this attribute"
            ></textarea>
          </div>

          <!-- Attribute Type -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Type</label>
            <select
              formControlName="attributeType"
              class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              @for (type of attributeTypes; track type.value) {
                <option [value]="type.value">{{ type.label }}</option>
              }
            </select>
          </div>

          <!-- Applied To -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Applied To</label>
            <div class="flex items-center gap-6">
              <label class="flex items-center gap-2 cursor-pointer">
                <input
                  type="radio"
                  formControlName="appliedTo"
                  value="contact"
                  class="h-4 w-4 border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <span class="text-sm text-gray-700">Contact</span>
              </label>
              <label class="flex items-center gap-2 cursor-pointer">
                <input
                  type="radio"
                  formControlName="appliedTo"
                  value="conversation"
                  class="h-4 w-4 border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <span class="text-sm text-gray-700">Conversation</span>
              </label>
            </div>
          </div>

          <!-- List Values (shown only when type is 'list') -->
          @if (attrForm.get('attributeType')?.value === 'list') {
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">List Values</label>
              <div class="space-y-2">
                @for (value of listValues; track $index) {
                  <div class="flex items-center gap-2">
                    <input
                      [value]="value"
                      (input)="updateListValue($index, $event)"
                      class="flex-1 rounded-lg border border-gray-300 px-3 py-1.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                      placeholder="Option value"
                    />
                    <button
                      type="button"
                      (click)="removeListValue($index)"
                      class="text-red-400 hover:text-red-600"
                    >
                      <svg class="h-5 w-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
                      </svg>
                    </button>
                  </div>
                }
                @if (listValues.length === 0) {
                  <p class="text-xs text-gray-400">No values added yet.</p>
                }
              </div>
              <button
                type="button"
                (click)="addListValue()"
                class="mt-2 px-3 py-1.5 text-xs font-medium text-blue-600 border border-blue-300 rounded-lg hover:bg-blue-50 transition-colors"
              >
                + Add Value
              </button>
            </div>
          }

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
              [disabled]="attrForm.invalid || (loading$ | async)"
              class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              {{ isEditing ? 'Update Attribute' : 'Create Attribute' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`:host { display: block; height: 100%; }`],
})
export class CustomAttributeFormComponent implements OnInit {
  private store = inject(Store);
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  loading$ = this.store.select(selectCustomAttributesLoading);
  attributeEntities$ = this.store.select(selectCustomAttributeEntities);

  attributeTypes = ATTRIBUTE_TYPES;
  isEditing = false;
  editingId: number | null = null;
  listValues: string[] = [];
  private keyManuallyEdited = false;

  attrForm: FormGroup = this.fb.group({
    displayName: ['', Validators.required],
    key: ['', Validators.required],
    description: [''],
    attributeType: ['text', Validators.required],
    appliedTo: ['contact', Validators.required],
  });

  ngOnInit(): void {
    this.store.dispatch(CustomAttributesActions.loadCustomAttributes());

    // Track manual key edits
    this.attrForm.get('key')?.valueChanges.subscribe(() => {
      this.keyManuallyEdited = true;
    });

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam && idParam !== 'new') {
      this.isEditing = true;
      this.editingId = Number(idParam);
      this.attributeEntities$.pipe(first((entities) => !!entities[this.editingId!])).subscribe((entities) => {
        const attr = entities[this.editingId!];
        if (attr) {
          this.keyManuallyEdited = true; // Don't auto-generate key when editing
          this.attrForm.patchValue({
            displayName: attr.displayName,
            key: attr.key,
            description: attr.description || '',
            attributeType: attr.attributeType,
            appliedTo: attr.appliedTo,
          });
          if (attr.listValues) {
            this.listValues = [...attr.listValues];
          }
        }
      });
    }
  }

  onNameInput(): void {
    if (this.keyManuallyEdited && this.isEditing) return;
    const name = this.attrForm.get('displayName')?.value || '';
    const key = name
      .toLowerCase()
      .replace(/[^a-z0-9\s]/g, '')
      .replace(/\s+/g, '_')
      .replace(/_+/g, '_')
      .replace(/^_|_$/g, '');
    this.keyManuallyEdited = false;
    this.attrForm.get('key')?.setValue(key, { emitEvent: false });
  }

  addListValue(): void {
    this.listValues = [...this.listValues, ''];
  }

  removeListValue(index: number): void {
    this.listValues = this.listValues.filter((_, i) => i !== index);
  }

  updateListValue(index: number, event: Event): void {
    const input = event.target as HTMLInputElement;
    this.listValues = this.listValues.map((v, i) => (i === index ? input.value : v));
  }

  onSubmit(): void {
    if (this.attrForm.invalid) return;

    const formValue = this.attrForm.value;
    const data: Record<string, unknown> = {
      displayName: formValue.displayName,
      key: formValue.key,
      description: formValue.description,
      attributeType: formValue.attributeType,
      appliedTo: formValue.appliedTo,
    };

    if (formValue.attributeType === 'list') {
      data['listValues'] = this.listValues.filter((v) => v.trim() !== '');
    }

    if (this.isEditing && this.editingId) {
      this.store.dispatch(CustomAttributesActions.updateCustomAttribute({ id: this.editingId, data }));
    } else {
      this.store.dispatch(CustomAttributesActions.createCustomAttribute({ data }));
    }
    this.goBack();
  }

  goBack(): void {
    this.router.navigate(['/settings/custom-attributes']);
  }
}
