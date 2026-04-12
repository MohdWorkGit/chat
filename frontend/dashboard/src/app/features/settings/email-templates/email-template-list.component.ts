import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService } from '@core/services/api.service';
import { BehaviorSubject } from 'rxjs';

interface EmailTemplate {
  id: number;
  name: string;
  templateType: string;
  locale: string;
  createdAt: string;
  updatedAt: string;
}

@Component({
  selector: 'app-email-template-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <div>
          <h2 class="text-lg font-semibold text-gray-900">Email Templates</h2>
          <p class="text-sm text-gray-500 mt-0.5">Customize the email templates sent to contacts.</p>
        </div>
        <button
          (click)="goToNew()"
          class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
        >
          New Template
        </button>
      </div>

      @if (loading$ | async) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if (templates$ | async; as templates) {
          @if (templates.length === 0) {
            <div class="text-center py-12 bg-white rounded-lg border border-gray-200">
              <svg class="mx-auto h-10 w-10 text-gray-400" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M21.75 6.75v10.5a2.25 2.25 0 0 1-2.25 2.25h-15a2.25 2.25 0 0 1-2.25-2.25V6.75m19.5 0A2.25 2.25 0 0 0 19.5 4.5h-15a2.25 2.25 0 0 0-2.25 2.25m19.5 0v.243a2.25 2.25 0 0 1-1.07 1.916l-7.5 4.615a2.25 2.25 0 0 1-2.36 0L3.32 8.91a2.25 2.25 0 0 1-1.07-1.916V6.75" />
              </svg>
              <p class="mt-2 text-sm text-gray-500">No email templates yet.</p>
              <button (click)="goToNew()" class="mt-4 text-sm text-blue-600 hover:text-blue-700">Create your first template</button>
            </div>
          } @else {
            <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Name</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Type</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Locale</th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Updated</th>
                    <th class="px-6 py-3"></th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-gray-200">
                  @for (tpl of templates; track tpl.id) {
                    <tr class="hover:bg-gray-50">
                      <td class="px-6 py-4 text-sm font-medium text-gray-900">{{ tpl.name }}</td>
                      <td class="px-6 py-4 text-sm text-gray-500">{{ tpl.templateType || '—' }}</td>
                      <td class="px-6 py-4 text-sm text-gray-500">{{ tpl.locale || 'en' }}</td>
                      <td class="px-6 py-4 text-sm text-gray-500">{{ tpl.updatedAt | date: 'mediumDate' }}</td>
                      <td class="px-6 py-4 text-right">
                        <button
                          (click)="goToEdit(tpl.id)"
                          class="text-sm text-blue-600 hover:text-blue-700 mr-4"
                        >Edit</button>
                        <button
                          (click)="deleteTemplate(tpl.id)"
                          class="text-sm text-red-600 hover:text-red-700"
                        >Delete</button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }
        }
      }
    </div>
  `,
  styles: [':host { display: block; height: 100%; }'],
})
export class EmailTemplateListComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);

  templates$ = new BehaviorSubject<EmailTemplate[]>([]);
  loading$ = new BehaviorSubject<boolean>(false);

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.loading$.next(true);
    this.api.get<EmailTemplate[]>(this.api.accountPath('/email_templates')).subscribe({
      next: (data) => { this.templates$.next(data); this.loading$.next(false); },
      error: () => this.loading$.next(false),
    });
  }

  goToNew(): void {
    this.router.navigate(['/settings/email-templates/new']);
  }

  goToEdit(id: number): void {
    this.router.navigate(['/settings/email-templates', id]);
  }

  deleteTemplate(id: number): void {
    if (!confirm('Delete this template?')) return;
    this.api.delete(this.api.accountPath(`/email_templates/${id}`)).subscribe({
      next: () => this.load(),
    });
  }
}
