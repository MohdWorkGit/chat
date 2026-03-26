import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { CustomAttributesActions } from '@store/custom-attributes/custom-attributes.actions';
import { selectAllCustomAttributes, selectCustomAttributesLoading } from '@store/custom-attributes/custom-attributes.selectors';
import { CustomAttribute } from '@core/models/custom-attribute.model';

const ATTRIBUTE_TYPE_LABELS: Record<string, string> = {
  text: 'Text',
  number: 'Number',
  date: 'Date',
  list: 'List',
  checkbox: 'Checkbox',
  link: 'Link',
  currency: 'Currency',
};

@Component({
  selector: 'app-custom-attributes-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-6">
        <div>
          <h2 class="text-lg font-semibold text-gray-900">Custom Attributes</h2>
          <p class="text-sm text-gray-500 mt-1">Define custom data fields for contacts and conversations.</p>
        </div>
        <button
          (click)="navigateToCreate()"
          class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
        >
          New Attribute
        </button>
      </div>

      <!-- Loading State -->
      @if (loading$ | async) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if ((attributes$ | async); as attributes) {
          <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
            @if (attributes.length > 0) {
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Key</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Applied To</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Created</th>
                    <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider"></th>
                  </tr>
                </thead>
                <tbody class="bg-white divide-y divide-gray-200">
                  @for (attr of attributes; track attr.id) {
                    <tr class="hover:bg-gray-50">
                      <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{{ attr.displayName }}</td>
                      <td class="px-6 py-4 whitespace-nowrap">
                        <code class="px-2 py-0.5 bg-gray-100 rounded text-xs font-mono text-blue-700">{{ attr.key }}</code>
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ getTypeLabel(attr.attributeType) }}</td>
                      <td class="px-6 py-4 whitespace-nowrap">
                        <span
                          class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium"
                          [class]="attr.appliedTo === 'contact' ? 'bg-green-100 text-green-800' : 'bg-purple-100 text-purple-800'"
                        >
                          {{ attr.appliedTo === 'contact' ? 'Contact' : 'Conversation' }}
                        </span>
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                        {{ attr.createdAt | date:'mediumDate' }}
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-right text-sm">
                        <button
                          (click)="navigateToEdit(attr.id)"
                          class="text-blue-600 hover:text-blue-800 mr-3"
                        >
                          Edit
                        </button>
                        <button
                          (click)="deleteAttribute(attr)"
                          class="text-red-600 hover:text-red-800"
                        >
                          Delete
                        </button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            } @else {
              <!-- Empty State -->
              <div class="text-center py-12">
                <h3 class="text-sm font-medium text-gray-900 mb-1">No custom attributes yet</h3>
                <p class="text-xs text-gray-500 mb-4">Create custom attributes to store additional data on contacts and conversations.</p>
                <button
                  (click)="navigateToCreate()"
                  class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
                >
                  New Attribute
                </button>
              </div>
            }
          </div>
        }
      }
    </div>
  `,
  styles: [`:host { display: block; height: 100%; }`],
})
export class CustomAttributesListComponent implements OnInit {
  private store = inject(Store);
  private router = inject(Router);

  attributes$ = this.store.select(selectAllCustomAttributes);
  loading$ = this.store.select(selectCustomAttributesLoading);

  ngOnInit(): void {
    this.store.dispatch(CustomAttributesActions.loadCustomAttributes());
  }

  getTypeLabel(type: string): string {
    return ATTRIBUTE_TYPE_LABELS[type] || type;
  }

  navigateToCreate(): void {
    this.router.navigate(['/settings/custom-attributes/new']);
  }

  navigateToEdit(id: number): void {
    this.router.navigate(['/settings/custom-attributes', id]);
  }

  deleteAttribute(attr: CustomAttribute): void {
    if (confirm(`Are you sure you want to delete the "${attr.displayName}" attribute?`)) {
      this.store.dispatch(CustomAttributesActions.deleteCustomAttribute({ id: attr.id }));
    }
  }
}
