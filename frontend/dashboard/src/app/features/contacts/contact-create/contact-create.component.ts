import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { ContactsActions } from '@app/store/contacts/contacts.actions';

@Component({
  selector: 'app-contact-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="h-full overflow-y-auto bg-gray-50">
      <div class="max-w-2xl mx-auto px-6 py-6">
        <!-- Header -->
        <div class="flex items-center justify-between mb-6">
          <div>
            <a routerLink="/contacts" class="text-sm text-blue-600 hover:text-blue-500 flex items-center gap-1 mb-2">
              <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5 8.25 12l7.5-7.5" />
              </svg>
              Back to Contacts
            </a>
            <h1 class="text-xl font-semibold text-gray-900">New Contact</h1>
          </div>
        </div>

        <!-- Form -->
        <div class="bg-white rounded-lg border border-gray-200 p-6">
          <form [formGroup]="contactForm" (ngSubmit)="onSubmit()" class="space-y-5">
            <!-- Name -->
            <div>
              <label class="block text-sm font-medium text-gray-700">Name <span class="text-red-500">*</span></label>
              <input
                formControlName="name"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="Contact name"
              />
              @if (contactForm.get('name')?.hasError('required') && contactForm.get('name')?.touched) {
                <p class="mt-1 text-xs text-red-500">Name is required</p>
              }
            </div>

            <div class="grid grid-cols-2 gap-4">
              <!-- Email -->
              <div>
                <label class="block text-sm font-medium text-gray-700">Email</label>
                <input
                  formControlName="email"
                  type="email"
                  class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="email@example.com"
                />
                @if (contactForm.get('email')?.hasError('email') && contactForm.get('email')?.touched) {
                  <p class="mt-1 text-xs text-red-500">Invalid email address</p>
                }
              </div>

              <!-- Phone -->
              <div>
                <label class="block text-sm font-medium text-gray-700">Phone</label>
                <input
                  formControlName="phone"
                  class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="+1 (555) 000-0000"
                />
              </div>
            </div>

            <div class="grid grid-cols-2 gap-4">
              <!-- Contact Type -->
              <div>
                <label class="block text-sm font-medium text-gray-700">Contact Type</label>
                <select
                  formControlName="contactType"
                  class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option value="visitor">Visitor</option>
                  <option value="lead">Lead</option>
                  <option value="customer">Customer</option>
                </select>
              </div>

              <!-- Company -->
              <div>
                <label class="block text-sm font-medium text-gray-700">Company</label>
                <input
                  formControlName="company"
                  class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="Company name"
                />
              </div>
            </div>

            <!-- Additional Info -->
            <div>
              <label class="block text-sm font-medium text-gray-700">Additional Information</label>
              <textarea
                formControlName="additionalInfo"
                rows="3"
                class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="Any additional notes about this contact..."
              ></textarea>
            </div>

            <!-- Actions -->
            <div class="flex items-center gap-3 pt-4 border-t border-gray-200">
              <button
                type="submit"
                [disabled]="contactForm.invalid"
                class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                Create Contact
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
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
    }
  `],
})
export class ContactCreateComponent {
  private store = inject(Store);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  contactForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    email: ['', Validators.email],
    phone: [''],
    contactType: ['lead'],
    company: [''],
    additionalInfo: [''],
  });

  onSubmit(): void {
    if (this.contactForm.invalid) return;

    const { name, email, phone, contactType, company, additionalInfo } = this.contactForm.value;
    const data: Record<string, unknown> = {
      name,
      email: email || undefined,
      phone: phone || undefined,
      contactType,
    };

    if (company) {
      data['company'] = { name: company };
    }

    if (additionalInfo) {
      data['additionalAttributes'] = { notes: additionalInfo };
    }

    this.store.dispatch(ContactsActions.createContact({ data }));
    this.router.navigate(['/contacts']);
  }

  onCancel(): void {
    this.router.navigate(['/contacts']);
  }
}
