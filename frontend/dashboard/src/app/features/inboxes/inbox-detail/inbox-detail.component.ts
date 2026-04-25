import { Component, inject, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { InboxesActions } from '@store/inboxes/inboxes.actions';
import { selectSelectedInbox, selectInboxesLoading } from '@store/inboxes/inboxes.selectors';
import { InboxService } from '@core/services/inbox.service';
import { PreChatFormField, WidgetConfig } from '@core/models/inbox.model';

@Component({
  selector: 'app-inbox-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, FormsModule],
  template: `
    <div class="p-6">
      <!-- Back Link -->
      <a routerLink="/settings/inboxes" class="inline-flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700 mb-6">
        <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5 8.25 12l7.5-7.5" />
        </svg>
        Back to Inboxes
      </a>

      @if (loading$ | async) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if (inbox$ | async; as inbox) {
          <div class="max-w-2xl">
            <h2 class="text-lg font-semibold text-gray-900 mb-6">{{ inbox.name }} Settings</h2>

            <form [formGroup]="inboxForm" (ngSubmit)="save()" class="space-y-6">
              <!-- Name -->
              <div>
                <label class="block text-sm font-medium text-gray-700">Inbox Name</label>
                <input
                  formControlName="name"
                  class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                />
              </div>

              <!-- Greeting Message -->
              <div>
                <label class="block text-sm font-medium text-gray-700">Greeting Message</label>
                <textarea
                  formControlName="greetingMessage"
                  rows="3"
                  class="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="Enter a greeting message for visitors..."
                ></textarea>
              </div>

              <!-- Auto Assignment Toggle -->
              <div class="flex items-center justify-between">
                <div>
                  <span class="text-sm font-medium text-gray-700">Enable Auto Assignment</span>
                  <p class="text-xs text-gray-400">Automatically assign conversations to available agents</p>
                </div>
                <div class="relative">
                  <input
                    type="checkbox"
                    formControlName="enableAutoAssignment"
                    class="sr-only peer"
                    id="auto-assignment"
                  />
                  <label
                    for="auto-assignment"
                    class="block w-10 h-6 rounded-full cursor-pointer transition-colors"
                    [class]="inboxForm.get('enableAutoAssignment')?.value ? 'bg-blue-600' : 'bg-gray-300'"
                  >
                    <span
                      class="block h-5 w-5 mt-0.5 rounded-full bg-white shadow transform transition-transform"
                      [class]="inboxForm.get('enableAutoAssignment')?.value ? 'translate-x-4.5 ml-0.5' : 'translate-x-0.5'"
                    ></span>
                  </label>
                </div>
              </div>

              <!-- CSAT Survey Toggle -->
              <div class="flex items-center justify-between">
                <div>
                  <span class="text-sm font-medium text-gray-700">CSAT Survey</span>
                  <p class="text-xs text-gray-400">Send a satisfaction survey after conversation is resolved</p>
                </div>
                <div class="relative">
                  <input
                    type="checkbox"
                    formControlName="csatSurveyEnabled"
                    class="sr-only peer"
                    id="csat-survey"
                  />
                  <label
                    for="csat-survey"
                    class="block w-10 h-6 rounded-full cursor-pointer transition-colors"
                    [class]="inboxForm.get('csatSurveyEnabled')?.value ? 'bg-blue-600' : 'bg-gray-300'"
                  >
                    <span
                      class="block h-5 w-5 mt-0.5 rounded-full bg-white shadow transform transition-transform"
                      [class]="inboxForm.get('csatSurveyEnabled')?.value ? 'translate-x-4.5 ml-0.5' : 'translate-x-0.5'"
                    ></span>
                  </label>
                </div>
              </div>

              <!-- Members Section -->
              <div class="border-t border-gray-200 pt-6">
                <h3 class="text-sm font-semibold text-gray-900 mb-4">Members</h3>
                @if (inbox.members && inbox.members.length > 0) {
                  <ul class="space-y-3">
                    @for (member of inbox.members; track member.id) {
                      <li class="flex items-center justify-between py-2">
                        <div class="flex items-center gap-3">
                          @if (member.userAvatar) {
                            <img [src]="member.userAvatar" class="h-8 w-8 rounded-full object-cover" />
                          } @else {
                            <div class="h-8 w-8 rounded-full bg-gray-300 flex items-center justify-center text-xs font-medium text-white">
                              {{ member.userName?.charAt(0)?.toUpperCase() || '?' }}
                            </div>
                          }
                          <span class="text-sm text-gray-700">{{ member.userName || 'Agent #' + member.userId }}</span>
                        </div>
                        <button
                          type="button"
                          (click)="removeMember(member.id)"
                          class="text-sm text-red-600 hover:text-red-800"
                        >
                          Remove
                        </button>
                      </li>
                    }
                  </ul>
                } @else {
                  <p class="text-sm text-gray-500">No members assigned to this inbox.</p>
                }
              </div>

              <!-- Widget Configuration Section -->
              @if (widgetConfig) {
                <div class="border-t border-gray-200 pt-6">
                  <h3 class="text-sm font-semibold text-gray-900 mb-4">Configuration</h3>

                  <div class="space-y-4">
                    <!-- Website Token -->
                    <div>
                      <label class="block text-sm font-medium text-gray-700 mb-1">Website Token</label>
                      <div class="flex items-center gap-2">
                        <code class="flex-1 block rounded-lg border border-gray-300 bg-gray-50 px-3 py-2 text-sm font-mono text-gray-800 select-all">{{ widgetConfig.websiteToken }}</code>
                        <button
                          type="button"
                          (click)="copyToClipboard(widgetConfig.websiteToken)"
                          class="shrink-0 px-3 py-2 text-sm font-medium text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
                        >
                          {{ tokenCopied ? 'Copied!' : 'Copy' }}
                        </button>
                      </div>
                    </div>

                    <!-- Embed Code -->
                    <div>
                      <label class="block text-sm font-medium text-gray-700 mb-1">Embed Code</label>
                      <p class="text-xs text-gray-400 mb-2">Copy and paste this code into your website's HTML before the closing &lt;/body&gt; tag</p>
                      <div class="relative">
                        <pre class="block rounded-lg border border-gray-300 bg-gray-50 px-3 py-3 text-xs font-mono text-gray-800 overflow-x-auto whitespace-pre-wrap break-all select-all">{{ getEmbedCode(widgetConfig.websiteToken) }}</pre>
                        <button
                          type="button"
                          (click)="copyToClipboard(getEmbedCode(widgetConfig.websiteToken))"
                          class="absolute top-2 right-2 px-2 py-1 text-xs font-medium text-gray-600 bg-white border border-gray-300 rounded hover:bg-gray-50 transition-colors"
                        >
                          {{ snippetCopied ? 'Copied!' : 'Copy' }}
                        </button>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Pre-Chat Form Builder -->
                <div class="border-t border-gray-200 pt-6">
                  <div class="flex items-center justify-between mb-4">
                    <div>
                      <h3 class="text-sm font-semibold text-gray-900">Pre-Chat Form</h3>
                      <p class="text-xs text-gray-400 mt-0.5">Collect visitor details before starting the conversation</p>
                    </div>
                    <div class="relative">
                      <input
                        type="checkbox"
                        [checked]="preChatEnabled"
                        (change)="togglePreChatEnabled($event)"
                        class="sr-only peer"
                        id="pre-chat-enabled"
                      />
                      <label
                        for="pre-chat-enabled"
                        class="block w-10 h-6 rounded-full cursor-pointer transition-colors"
                        [class]="preChatEnabled ? 'bg-blue-600' : 'bg-gray-300'"
                      >
                        <span
                          class="block h-5 w-5 mt-0.5 rounded-full bg-white shadow transform transition-transform"
                          [class]="preChatEnabled ? 'translate-x-4.5 ml-0.5' : 'translate-x-0.5'"
                        ></span>
                      </label>
                    </div>
                  </div>

                  @if (preChatEnabled) {
                    <div class="space-y-3">
                      @if (preChatFields.length === 0) {
                        <p class="text-xs text-gray-500">No fields yet. Add one below.</p>
                      }
                      @for (field of preChatFields; track $index; let i = $index) {
                        <div class="rounded-lg border border-gray-200 p-3 bg-gray-50">
                          <div class="grid grid-cols-12 gap-2 items-center">
                            <input
                              [(ngModel)]="field.label"
                              [ngModelOptions]="{ standalone: true }"
                              (ngModelChange)="markPreChatDirty()"
                              placeholder="Label"
                              class="col-span-4 rounded border border-gray-300 px-2 py-1 text-sm focus:outline-none focus:ring-1 focus:ring-blue-500"
                            />
                            <input
                              [(ngModel)]="field.name"
                              [ngModelOptions]="{ standalone: true }"
                              (ngModelChange)="markPreChatDirty()"
                              placeholder="Name (key)"
                              class="col-span-3 rounded border border-gray-300 px-2 py-1 text-sm font-mono focus:outline-none focus:ring-1 focus:ring-blue-500"
                            />
                            <select
                              [(ngModel)]="field.type"
                              [ngModelOptions]="{ standalone: true }"
                              (ngModelChange)="markPreChatDirty()"
                              class="col-span-2 rounded border border-gray-300 px-2 py-1 text-sm focus:outline-none focus:ring-1 focus:ring-blue-500"
                            >
                              <option value="text">Text</option>
                              <option value="email">Email</option>
                              <option value="number">Number</option>
                              <option value="checkbox">Checkbox</option>
                            </select>
                            <label class="col-span-2 flex items-center gap-1 text-xs text-gray-600">
                              <input
                                type="checkbox"
                                [(ngModel)]="field.required"
                                [ngModelOptions]="{ standalone: true }"
                                (ngModelChange)="markPreChatDirty()"
                                class="rounded border-gray-300"
                              />
                              Required
                            </label>
                            <button
                              type="button"
                              (click)="removePreChatField(i)"
                              class="col-span-1 text-red-600 hover:text-red-800 text-sm"
                              title="Remove"
                            >
                              &times;
                            </button>
                          </div>
                        </div>
                      }

                      <div class="flex items-center justify-between pt-2">
                        <button
                          type="button"
                          (click)="addPreChatField()"
                          class="text-sm font-medium text-blue-600 hover:text-blue-800"
                        >
                          + Add field
                        </button>
                        <button
                          type="button"
                          (click)="savePreChat()"
                          [disabled]="!preChatDirty || preChatSaving"
                          class="px-4 py-1.5 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                        >
                          {{ preChatSaving ? 'Saving...' : 'Save pre-chat form' }}
                        </button>
                      </div>
                    </div>
                  } @else {
                    <div class="flex justify-end">
                      <button
                        type="button"
                        (click)="savePreChat()"
                        [disabled]="!preChatDirty || preChatSaving"
                        class="px-4 py-1.5 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                      >
                        {{ preChatSaving ? 'Saving...' : 'Save' }}
                      </button>
                    </div>
                  }
                </div>
              }

              <!-- Actions -->
              <div class="flex items-center justify-between pt-6 border-t border-gray-200">
                <button
                  type="button"
                  (click)="deleteInbox()"
                  class="px-4 py-2 text-sm font-medium text-red-600 border border-red-300 rounded-lg hover:bg-red-50 transition-colors"
                >
                  Delete Inbox
                </button>
                <button
                  type="submit"
                  [disabled]="inboxForm.invalid || inboxForm.pristine"
                  class="px-6 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                >
                  Save Changes
                </button>
              </div>
            </form>
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
export class InboxDetailComponent implements OnInit {
  @Input() id!: string;

  private store = inject(Store);
  private fb = inject(FormBuilder);
  private inboxService = inject(InboxService);

  inbox$ = this.store.select(selectSelectedInbox);
  loading$ = this.store.select(selectInboxesLoading);

  widgetConfig: WidgetConfig | null = null;
  tokenCopied = false;
  snippetCopied = false;

  preChatEnabled = false;
  preChatFields: PreChatFormField[] = [];
  preChatDirty = false;
  preChatSaving = false;

  inboxForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    greetingMessage: [''],
    enableAutoAssignment: [false],
    csatSurveyEnabled: [false],
  });

  ngOnInit(): void {
    this.store.dispatch(InboxesActions.loadInbox({ id: Number(this.id) }));

    this.inbox$.subscribe((inbox) => {
      if (inbox) {
        this.inboxForm.patchValue({
          name: inbox.name,
          greetingMessage: inbox.greetingMessage || '',
          enableAutoAssignment: inbox.enableAutoAssignment,
          csatSurveyEnabled: inbox.csatSurveyEnabled,
        });

        if (inbox.channelType === 'web_widget') {
          this.loadWidgetConfig();
        }
      }
    });
  }

  save(): void {
    if (this.inboxForm.invalid) return;
    this.store.dispatch(
      InboxesActions.updateInbox({
        id: Number(this.id),
        data: this.inboxForm.value,
      })
    );
    this.inboxForm.markAsPristine();
  }

  deleteInbox(): void {
    if (confirm('Are you sure you want to delete this inbox? This action cannot be undone.')) {
      this.store.dispatch(InboxesActions.deleteInbox({ id: Number(this.id) }));
    }
  }

  removeMember(memberId: number): void {
    this.store.dispatch(
      InboxesActions.removeMember({ inboxId: Number(this.id), memberId })
    );
  }

  private loadWidgetConfig(): void {
    if (this.widgetConfig) return;
    this.inboxService.getWidgetConfig(Number(this.id)).subscribe({
      next: (config) => {
        this.widgetConfig = config;
        this.hydratePreChatFromConfig(config);
      },
      error: () => (this.widgetConfig = null),
    });
  }

  private hydratePreChatFromConfig(config: WidgetConfig): void {
    this.preChatEnabled = !!config.preChatFormEnabled;
    this.preChatFields = this.parsePreChatFields(config.preChatFormOptions);
    this.preChatDirty = false;
  }

  private parsePreChatFields(raw: string | null | undefined): PreChatFormField[] {
    if (!raw) return [];
    try {
      const parsed = JSON.parse(raw);
      if (!Array.isArray(parsed)) return [];
      return parsed
        .filter((f): f is PreChatFormField => f && typeof f === 'object')
        .sort((a, b) => (a.position ?? 0) - (b.position ?? 0));
    } catch {
      return [];
    }
  }

  togglePreChatEnabled(event: Event): void {
    this.preChatEnabled = (event.target as HTMLInputElement).checked;
    this.markPreChatDirty();
  }

  addPreChatField(): void {
    this.preChatFields = [
      ...this.preChatFields,
      {
        name: `field_${this.preChatFields.length + 1}`,
        label: 'New field',
        type: 'text',
        required: false,
        enabled: true,
        position: this.preChatFields.length,
      },
    ];
    this.markPreChatDirty();
  }

  removePreChatField(index: number): void {
    this.preChatFields = this.preChatFields
      .filter((_, i) => i !== index)
      .map((f, i) => ({ ...f, position: i }));
    this.markPreChatDirty();
  }

  markPreChatDirty(): void {
    this.preChatDirty = true;
  }

  savePreChat(): void {
    if (this.preChatSaving) return;

    const payload = {
      preChatFormEnabled: this.preChatEnabled,
      preChatFormFields: this.preChatFields.map((f, i) => ({
        name: (f.name || '').trim(),
        label: (f.label || '').trim(),
        type: f.type,
        required: !!f.required,
        enabled: f.enabled !== false,
        position: i,
      })),
    };

    const invalid = payload.preChatFormFields.find((f) => !f.name || !f.label);
    if (invalid) {
      alert('Every pre-chat field needs a name and label.');
      return;
    }

    this.preChatSaving = true;
    this.inboxService.updateWidgetConfig(Number(this.id), payload).subscribe({
      next: (config) => {
        this.widgetConfig = config;
        this.hydratePreChatFromConfig(config);
        this.preChatSaving = false;
      },
      error: () => {
        this.preChatSaving = false;
        alert('Failed to save pre-chat form. Please try again.');
      },
    });
  }

  getEmbedCode(token: string): string {
    return `<script>\n  (function(d, t) {\n    var g = d.createElement(t), s = d.getElementsByTagName(t)[0];\n    g.src = "${window.location.origin}/widget/widget.js";\n    g.defer = true;\n    g.async = true;\n    s.parentNode.insertBefore(g, s);\n    g.onload = function() {\n      window.customerEngagementSettings = {\n        websiteToken: "${token}"\n      };\n    };\n  })(document, "script");\n</script>`;
  }

  copyToClipboard(text: string): void {
    navigator.clipboard.writeText(text).then(() => {
      if (text.includes('<script>')) {
        this.snippetCopied = true;
        setTimeout(() => (this.snippetCopied = false), 2000);
      } else {
        this.tokenCopied = true;
        setTimeout(() => (this.tokenCopied = false), 2000);
      }
    });
  }
}
