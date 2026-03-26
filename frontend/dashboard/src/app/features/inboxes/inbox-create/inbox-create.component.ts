import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { InboxesActions } from '@app/store/inboxes/inboxes.actions';

@Component({
  selector: 'app-inbox-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="h-full overflow-y-auto bg-gray-50">
      <div class="max-w-2xl mx-auto px-6 py-6">
        <!-- Header -->
        <div class="flex items-center justify-between mb-6">
          <div>
            <a routerLink="/settings/inboxes" class="text-sm text-blue-600 hover:text-blue-500 flex items-center gap-1 mb-2">
              <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5 8.25 12l7.5-7.5" />
              </svg>
              Back to Inboxes
            </a>
            <h1 class="text-xl font-semibold text-gray-900">New Inbox</h1>
          </div>
        </div>

        <!-- Step Indicator -->
        <div class="flex items-center gap-2 mb-6">
          @for (s of steps; track s.number) {
            <div class="flex items-center gap-2">
              <div
                class="h-8 w-8 rounded-full flex items-center justify-center text-sm font-medium transition-colors"
                [class]="currentStep >= s.number
                  ? 'bg-blue-600 text-white'
                  : 'bg-gray-200 text-gray-500'"
              >
                @if (currentStep > s.number) {
                  <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" d="m4.5 12.75 6 6 9-13.5" />
                  </svg>
                } @else {
                  {{ s.number }}
                }
              </div>
              <span
                class="text-sm font-medium"
                [class]="currentStep >= s.number ? 'text-gray-900' : 'text-gray-400'"
              >
                {{ s.label }}
              </span>
              @if (s.number < steps.length) {
                <div class="w-12 h-px bg-gray-300 mx-1"></div>
              }
            </div>
          }
        </div>

        <!-- Step 1: Channel Type -->
        @if (currentStep === 1) {
          <div class="bg-white rounded-lg border border-gray-200 p-6">
            <h3 class="text-sm font-semibold text-gray-900 mb-4">Select Channel Type</h3>
            <div class="grid grid-cols-3 gap-4">
              @for (channel of channelTypes; track channel.value) {
                <button
                  type="button"
                  (click)="selectChannelType(channel.value)"
                  class="flex flex-col items-center gap-3 p-6 rounded-lg border-2 transition-colors"
                  [class]="selectedChannelType === channel.value
                    ? 'border-blue-500 bg-blue-50'
                    : 'border-gray-200 hover:border-gray-300 hover:bg-gray-50'"
                >
                  <div
                    class="h-12 w-12 rounded-lg flex items-center justify-center"
                    [class]="selectedChannelType === channel.value ? 'bg-blue-100' : 'bg-gray-100'"
                  >
                    <svg class="h-6 w-6" [class]="selectedChannelType === channel.value ? 'text-blue-600' : 'text-gray-500'" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                      @if (channel.value === 'web_widget') {
                        <path stroke-linecap="round" stroke-linejoin="round" d="M20.25 8.511c.884.284 1.5 1.128 1.5 2.097v4.286c0 1.136-.847 2.1-1.98 2.193-.34.027-.68.052-1.02.072v3.091l-3-3c-1.354 0-2.694-.055-4.02-.163a2.115 2.115 0 0 1-.825-.242m9.345-8.334a2.126 2.126 0 0 0-.476-.095 48.64 48.64 0 0 0-8.048 0c-1.131.094-1.976 1.057-1.976 2.192v4.286c0 .837.46 1.58 1.155 1.951m9.345-8.334V6.637c0-1.621-1.152-3.026-2.76-3.235A48.455 48.455 0 0 0 11.25 3c-2.115 0-4.198.137-6.24.402-1.608.209-2.76 1.614-2.76 3.235v6.226c0 1.621 1.152 3.026 2.76 3.235.577.075 1.157.14 1.74.194V21l4.155-4.155" />
                      } @else if (channel.value === 'email') {
                        <path stroke-linecap="round" stroke-linejoin="round" d="M21.75 6.75v10.5a2.25 2.25 0 0 1-2.25 2.25h-15a2.25 2.25 0 0 1-2.25-2.25V6.75m19.5 0A2.25 2.25 0 0 0 19.5 4.5h-15a2.25 2.25 0 0 0-2.25 2.25m19.5 0v.243a2.25 2.25 0 0 1-1.07 1.916l-7.5 4.615a2.25 2.25 0 0 1-2.36 0L3.32 8.91a2.25 2.25 0 0 1-1.07-1.916V6.75" />
                      } @else {
                        <path stroke-linecap="round" stroke-linejoin="round" d="M17.25 6.75 22.5 12l-5.25 5.25m-10.5 0L1.5 12l5.25-5.25m7.5-3-4.5 16.5" />
                      }
                    </svg>
                  </div>
                  <div class="text-center">
                    <p class="text-sm font-medium" [class]="selectedChannelType === channel.value ? 'text-blue-700' : 'text-gray-900'">
                      {{ channel.label }}
                    </p>
                    <p class="text-xs text-gray-400 mt-0.5">{{ channel.description }}</p>
                  </div>
                </button>
              }
            </div>
            <div class="flex justify-end mt-6">
              <button
                type="button"
                (click)="nextStep()"
                [disabled]="!selectedChannelType"
                class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                Next
              </button>
            </div>
          </div>
        }

        <!-- Step 2: Channel Configuration -->
        @if (currentStep === 2) {
          <div class="bg-white rounded-lg border border-gray-200 p-6">
            <h3 class="text-sm font-semibold text-gray-900 mb-4">Configure {{ getChannelLabel() }}</h3>

            <!-- Web Widget Config -->
            @if (selectedChannelType === 'web_widget') {
              <form [formGroup]="webWidgetForm" class="space-y-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700">Name <span class="text-red-500">*</span></label>
                  <input
                    formControlName="name"
                    class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    placeholder="My Website Widget"
                  />
                  @if (webWidgetForm.get('name')?.hasError('required') && webWidgetForm.get('name')?.touched) {
                    <p class="mt-1 text-xs text-red-500">Name is required</p>
                  }
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">Greeting Message</label>
                  <textarea
                    formControlName="greetingMessage"
                    rows="2"
                    class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    placeholder="Hi! How can we help you today?"
                  ></textarea>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">Widget Color</label>
                  <div class="mt-1 flex items-center gap-3">
                    <input
                      formControlName="widgetColor"
                      type="color"
                      class="h-10 w-10 rounded border border-gray-300 cursor-pointer"
                    />
                    <input
                      formControlName="widgetColor"
                      class="w-32 rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                      placeholder="#1F93FF"
                    />
                  </div>
                </div>
              </form>
            }

            <!-- Email Config -->
            @if (selectedChannelType === 'email') {
              <form [formGroup]="emailForm" class="space-y-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700">Name <span class="text-red-500">*</span></label>
                  <input
                    formControlName="name"
                    class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    placeholder="Support Email"
                  />
                  @if (emailForm.get('name')?.hasError('required') && emailForm.get('name')?.touched) {
                    <p class="mt-1 text-xs text-red-500">Name is required</p>
                  }
                </div>
                <div class="border-t border-gray-200 pt-4">
                  <h4 class="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-3">IMAP Settings</h4>
                  <div class="grid grid-cols-2 gap-4">
                    <div>
                      <label class="block text-sm font-medium text-gray-700">IMAP Server</label>
                      <input
                        formControlName="imapServer"
                        class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                        placeholder="imap.example.com"
                      />
                    </div>
                    <div>
                      <label class="block text-sm font-medium text-gray-700">IMAP Port</label>
                      <input
                        formControlName="imapPort"
                        type="number"
                        class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                        placeholder="993"
                      />
                    </div>
                  </div>
                </div>
                <div class="border-t border-gray-200 pt-4">
                  <h4 class="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-3">SMTP Settings</h4>
                  <div class="grid grid-cols-2 gap-4">
                    <div>
                      <label class="block text-sm font-medium text-gray-700">SMTP Server</label>
                      <input
                        formControlName="smtpServer"
                        class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                        placeholder="smtp.example.com"
                      />
                    </div>
                    <div>
                      <label class="block text-sm font-medium text-gray-700">SMTP Port</label>
                      <input
                        formControlName="smtpPort"
                        type="number"
                        class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                        placeholder="587"
                      />
                    </div>
                  </div>
                </div>
                <div class="grid grid-cols-2 gap-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700">Email Address</label>
                    <input
                      formControlName="emailAddress"
                      type="email"
                      class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                      placeholder="support@example.com"
                    />
                  </div>
                  <div>
                    <label class="block text-sm font-medium text-gray-700">Password</label>
                    <input
                      formControlName="emailPassword"
                      type="password"
                      class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                      placeholder="App password"
                    />
                  </div>
                </div>
              </form>
            }

            <!-- API Config -->
            @if (selectedChannelType === 'api') {
              <form [formGroup]="apiForm" class="space-y-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700">Name <span class="text-red-500">*</span></label>
                  <input
                    formControlName="name"
                    class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    placeholder="API Channel"
                  />
                  @if (apiForm.get('name')?.hasError('required') && apiForm.get('name')?.touched) {
                    <p class="mt-1 text-xs text-red-500">Name is required</p>
                  }
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-700">Webhook URL</label>
                  <input
                    formControlName="webhookUrl"
                    type="url"
                    class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                    placeholder="https://example.com/webhook"
                  />
                  <p class="mt-1 text-xs text-gray-400">We will send events to this URL when new messages are received.</p>
                </div>
              </form>
            }

            <div class="flex items-center justify-between mt-6 pt-4 border-t border-gray-200">
              <button
                type="button"
                (click)="previousStep()"
                class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
              >
                Back
              </button>
              <button
                type="button"
                (click)="nextStep()"
                [disabled]="!isCurrentFormValid()"
                class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                Next
              </button>
            </div>
          </div>
        }

        <!-- Step 3: Confirmation -->
        @if (currentStep === 3) {
          <div class="bg-white rounded-lg border border-gray-200 p-6">
            <h3 class="text-sm font-semibold text-gray-900 mb-4">Confirm Inbox Setup</h3>
            <div class="space-y-3">
              <div class="flex items-center justify-between py-2 border-b border-gray-100">
                <span class="text-sm text-gray-500">Channel Type</span>
                <span class="text-sm font-medium text-gray-900">{{ getChannelLabel() }}</span>
              </div>
              <div class="flex items-center justify-between py-2 border-b border-gray-100">
                <span class="text-sm text-gray-500">Name</span>
                <span class="text-sm font-medium text-gray-900">{{ getActiveForm().get('name')?.value }}</span>
              </div>
              @if (selectedChannelType === 'web_widget' && webWidgetForm.get('greetingMessage')?.value) {
                <div class="flex items-center justify-between py-2 border-b border-gray-100">
                  <span class="text-sm text-gray-500">Greeting</span>
                  <span class="text-sm text-gray-900">{{ webWidgetForm.get('greetingMessage')?.value }}</span>
                </div>
              }
              @if (selectedChannelType === 'email') {
                <div class="flex items-center justify-between py-2 border-b border-gray-100">
                  <span class="text-sm text-gray-500">Email</span>
                  <span class="text-sm text-gray-900">{{ emailForm.get('emailAddress')?.value || 'Not set' }}</span>
                </div>
              }
              @if (selectedChannelType === 'api' && apiForm.get('webhookUrl')?.value) {
                <div class="flex items-center justify-between py-2 border-b border-gray-100">
                  <span class="text-sm text-gray-500">Webhook URL</span>
                  <span class="text-sm text-gray-900 truncate max-w-xs">{{ apiForm.get('webhookUrl')?.value }}</span>
                </div>
              }
            </div>

            <div class="flex items-center justify-between mt-6 pt-4 border-t border-gray-200">
              <button
                type="button"
                (click)="previousStep()"
                class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
              >
                Back
              </button>
              <button
                type="button"
                (click)="onSubmit()"
                class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
              >
                Create Inbox
              </button>
            </div>
          </div>
        }
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
export class InboxCreateComponent {
  private store = inject(Store);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  currentStep = 1;
  selectedChannelType = '';

  steps = [
    { number: 1, label: 'Channel Type' },
    { number: 2, label: 'Configuration' },
    { number: 3, label: 'Confirmation' },
  ];

  channelTypes = [
    { value: 'web_widget', label: 'Web Widget', description: 'Add a chat widget to your website' },
    { value: 'email', label: 'Email', description: 'Connect an email account' },
    { value: 'api', label: 'API', description: 'Integrate via API and webhooks' },
  ];

  webWidgetForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    greetingMessage: [''],
    widgetColor: ['#1F93FF'],
  });

  emailForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    imapServer: [''],
    imapPort: [993],
    smtpServer: [''],
    smtpPort: [587],
    emailAddress: ['', Validators.email],
    emailPassword: [''],
  });

  apiForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    webhookUrl: [''],
  });

  selectChannelType(type: string): void {
    this.selectedChannelType = type;
  }

  getChannelLabel(): string {
    return this.channelTypes.find((c) => c.value === this.selectedChannelType)?.label ?? '';
  }

  getActiveForm(): FormGroup {
    switch (this.selectedChannelType) {
      case 'web_widget':
        return this.webWidgetForm;
      case 'email':
        return this.emailForm;
      case 'api':
        return this.apiForm;
      default:
        return this.webWidgetForm;
    }
  }

  isCurrentFormValid(): boolean {
    return this.getActiveForm().valid;
  }

  nextStep(): void {
    if (this.currentStep < 3) {
      this.currentStep++;
    }
  }

  previousStep(): void {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }

  onSubmit(): void {
    const form = this.getActiveForm();
    if (form.invalid) return;

    const formValue = form.value;
    const data: Record<string, unknown> = {
      name: formValue.name,
      channelType: this.selectedChannelType,
    };

    if (this.selectedChannelType === 'web_widget') {
      data['greeting'] = !!formValue.greetingMessage;
      data['greetingMessage'] = formValue.greetingMessage || undefined;
    } else if (this.selectedChannelType === 'email') {
      data['channel'] = {
        imapServer: formValue.imapServer,
        imapPort: formValue.imapPort,
        smtpServer: formValue.smtpServer,
        smtpPort: formValue.smtpPort,
        emailAddress: formValue.emailAddress,
        emailPassword: formValue.emailPassword,
      };
    } else if (this.selectedChannelType === 'api') {
      data['channel'] = {
        webhookUrl: formValue.webhookUrl,
      };
    }

    this.store.dispatch(InboxesActions.createInbox({ data }));
    this.router.navigate(['/settings/inboxes']);
  }
}
