import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  CaptainService,
  ConversationSummary,
  RewriteResult,
} from '@core/services/captain.service';

type Tab = 'suggest' | 'summary' | 'rewrite' | 'labels' | 'followup';

interface PanelState {
  loading: boolean;
  error: string | null;
}

@Component({
  selector: 'app-copilot-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="flex flex-col h-full bg-white border-l border-gray-200 w-80 flex-shrink-0">
      <!-- Header -->
      <div class="flex items-center justify-between px-4 py-3 border-b border-gray-200">
        <div class="flex items-center gap-2">
          <div class="h-7 w-7 rounded-full bg-gradient-to-br from-purple-500 to-blue-500 flex items-center justify-center">
            <svg class="h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M9.813 15.904L9 18.75l-.813-2.846a4.5 4.5 0 00-3.09-3.09L2.25 12l2.846-.813a4.5 4.5 0 003.09-3.09L9 5.25l.813 2.846a4.5 4.5 0 003.09 3.09L15.75 12l-2.846.813a4.5 4.5 0 00-3.09 3.09z" />
            </svg>
          </div>
          <h4 class="text-sm font-semibold text-gray-900">Copilot</h4>
        </div>
        <button
          (click)="closed.emit()"
          class="text-gray-400 hover:text-gray-600"
          title="Close"
        >
          <svg class="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="M6 18 18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <!-- Tabs -->
      <div class="flex items-center border-b border-gray-200 text-xs">
        @for (t of tabs; track t.key) {
          <button
            (click)="setTab(t.key)"
            class="flex-1 px-2 py-2.5 font-medium transition-colors"
            [class]="activeTab === t.key
              ? 'text-blue-600 border-b-2 border-blue-600'
              : 'text-gray-500 hover:text-gray-700'"
          >
            {{ t.label }}
          </button>
        }
      </div>

      <!-- Content -->
      <div class="flex-1 overflow-y-auto p-4 text-sm">
        @if (!conversationId) {
          <div class="text-center text-gray-400 py-8">
            <p class="text-xs">Select a conversation to use Copilot.</p>
          </div>
        } @else {
          @switch (activeTab) {
            @case ('suggest') {
              <div>
                <button
                  (click)="runSuggestReply()"
                  [disabled]="state.loading"
                  class="w-full px-3 py-2 text-xs font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50"
                >
                  {{ state.loading ? 'Generating...' : 'Suggest reply' }}
                </button>
                @if (suggestedReply) {
                  <div class="mt-3 rounded-lg bg-blue-50 border border-blue-100 p-3">
                    <p class="text-xs text-gray-700 whitespace-pre-wrap">{{ suggestedReply }}</p>
                    <button
                      (click)="copyToClipboard(suggestedReply)"
                      class="mt-2 text-xs text-blue-600 hover:text-blue-800 font-medium"
                    >
                      Copy to clipboard
                    </button>
                  </div>
                }
              </div>
            }

            @case ('summary') {
              <div>
                <button
                  (click)="runSummarize()"
                  [disabled]="state.loading"
                  class="w-full px-3 py-2 text-xs font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50"
                >
                  {{ state.loading ? 'Summarizing...' : 'Summarize conversation' }}
                </button>
                @if (summary) {
                  <div class="mt-3 space-y-3">
                    <div class="rounded-lg bg-gray-50 border border-gray-200 p-3">
                      <h5 class="text-xs font-semibold text-gray-700 mb-1">Summary</h5>
                      <p class="text-xs text-gray-600 whitespace-pre-wrap">{{ summary.summary }}</p>
                    </div>
                    @if (summary.keyPoints.length) {
                      <div class="rounded-lg bg-gray-50 border border-gray-200 p-3">
                        <h5 class="text-xs font-semibold text-gray-700 mb-2">Key points</h5>
                        <ul class="text-xs text-gray-600 list-disc list-inside space-y-1">
                          @for (kp of summary.keyPoints; track kp) {
                            <li>{{ kp }}</li>
                          }
                        </ul>
                      </div>
                    }
                  </div>
                }
              </div>
            }

            @case ('rewrite') {
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">Text</label>
                <textarea
                  [(ngModel)]="rewriteInput"
                  rows="4"
                  class="w-full rounded-lg border border-gray-300 px-3 py-2 text-xs focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="Paste or type the reply you want to rewrite..."
                ></textarea>
                <label class="block text-xs font-medium text-gray-700 mt-3 mb-1">Tone</label>
                <select
                  [(ngModel)]="rewriteTone"
                  class="w-full rounded-lg border border-gray-300 px-3 py-2 text-xs bg-white focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option value="professional">Professional</option>
                  <option value="friendly">Friendly</option>
                  <option value="empathetic">Empathetic</option>
                  <option value="concise">Concise</option>
                  <option value="formal">Formal</option>
                </select>
                <button
                  (click)="runRewrite()"
                  [disabled]="state.loading || !rewriteInput.trim()"
                  class="w-full mt-3 px-3 py-2 text-xs font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50"
                >
                  {{ state.loading ? 'Rewriting...' : 'Rewrite' }}
                </button>
                @if (rewriteResult) {
                  <div class="mt-3 rounded-lg bg-blue-50 border border-blue-100 p-3">
                    <p class="text-xs text-gray-700 whitespace-pre-wrap">{{ rewriteResult.rewritten }}</p>
                    <button
                      (click)="copyToClipboard(rewriteResult.rewritten)"
                      class="mt-2 text-xs text-blue-600 hover:text-blue-800 font-medium"
                    >
                      Copy to clipboard
                    </button>
                  </div>
                }
              </div>
            }

            @case ('labels') {
              <div>
                <button
                  (click)="runSuggestLabels()"
                  [disabled]="state.loading"
                  class="w-full px-3 py-2 text-xs font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50"
                >
                  {{ state.loading ? 'Analyzing...' : 'Suggest labels' }}
                </button>
                @if (suggestedLabels.length) {
                  <div class="mt-3 flex flex-wrap gap-1.5">
                    @for (label of suggestedLabels; track label) {
                      <span class="inline-flex items-center px-2 py-1 rounded text-xs bg-purple-100 text-purple-800 font-medium">
                        {{ label }}
                      </span>
                    }
                  </div>
                }
              </div>
            }

            @case ('followup') {
              <div>
                <button
                  (click)="runSuggestFollowUp()"
                  [disabled]="state.loading"
                  class="w-full px-3 py-2 text-xs font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50"
                >
                  {{ state.loading ? 'Thinking...' : 'Suggest follow-ups' }}
                </button>
                @if (followUps.length) {
                  <ul class="mt-3 space-y-2">
                    @for (f of followUps; track f) {
                      <li class="rounded-lg bg-gray-50 border border-gray-200 p-2.5 text-xs text-gray-700 flex items-start justify-between gap-2">
                        <span class="flex-1">{{ f }}</span>
                        <button
                          (click)="copyToClipboard(f)"
                          class="text-blue-600 hover:text-blue-800 shrink-0"
                          title="Copy"
                        >
                          <svg class="h-3.5 w-3.5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 17.25v3.375c0 .621-.504 1.125-1.125 1.125h-9.75a1.125 1.125 0 0 1-1.125-1.125V7.875c0-.621.504-1.125 1.125-1.125H6.75a9.06 9.06 0 0 1 1.5.124m7.5 10.376h3.375c.621 0 1.125-.504 1.125-1.125V11.25c0-4.46-3.243-8.161-7.5-8.876a9.06 9.06 0 0 0-1.5-.124H9.375c-.621 0-1.125.504-1.125 1.125v3.5m7.5 10.375H9.375a1.125 1.125 0 0 1-1.125-1.125v-9.25m12 6.625v-1.875a3.375 3.375 0 0 0-3.375-3.375h-1.5a1.125 1.125 0 0 1-1.125-1.125v-1.5a3.375 3.375 0 0 0-3.375-3.375H9.75" />
                          </svg>
                        </button>
                      </li>
                    }
                  </ul>
                }
              </div>
            }
          }

          @if (state.error) {
            <div class="mt-3 rounded-lg bg-red-50 border border-red-100 p-3 text-xs text-red-700">
              {{ state.error }}
            </div>
          }
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
export class CopilotPanelComponent implements OnChanges {
  @Input() conversationId: number | null = null;
  @Input() accountId = 1;

  private captain = inject(CaptainService);

  readonly tabs: { key: Tab; label: string }[] = [
    { key: 'suggest', label: 'Reply' },
    { key: 'summary', label: 'Summary' },
    { key: 'rewrite', label: 'Rewrite' },
    { key: 'labels', label: 'Labels' },
    { key: 'followup', label: 'Follow-up' },
  ];

  activeTab: Tab = 'suggest';
  state: PanelState = { loading: false, error: null };

  suggestedReply = '';
  summary: ConversationSummary | null = null;
  rewriteInput = '';
  rewriteTone = 'professional';
  rewriteResult: RewriteResult | null = null;
  suggestedLabels: string[] = [];
  followUps: string[] = [];

  @Output() readonly closed = new EventEmitter<void>();

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['conversationId']) {
      this.resetResults();
    }
  }

  setTab(tab: Tab): void {
    this.activeTab = tab;
    this.state.error = null;
  }

  runSuggestReply(): void {
    if (!this.conversationId) return;
    this.begin();
    this.captain.suggestReply(this.accountId, this.conversationId).subscribe({
      next: (res) => {
        this.suggestedReply = res.text;
        this.end();
      },
      error: () => this.fail('Unable to generate suggestion.'),
    });
  }

  runSummarize(): void {
    if (!this.conversationId) return;
    this.begin();
    this.captain.summarizeConversation(this.accountId, this.conversationId).subscribe({
      next: (res) => {
        this.summary = res;
        this.end();
      },
      error: () => this.fail('Unable to summarize conversation.'),
    });
  }

  runRewrite(): void {
    if (!this.rewriteInput.trim()) return;
    this.begin();
    this.captain.rewriteText(this.accountId, this.rewriteInput, this.rewriteTone).subscribe({
      next: (res) => {
        this.rewriteResult = res;
        this.end();
      },
      error: () => this.fail('Unable to rewrite text.'),
    });
  }

  runSuggestLabels(): void {
    if (!this.conversationId) return;
    this.begin();
    this.captain.suggestLabels(this.accountId, this.conversationId).subscribe({
      next: (res) => {
        this.suggestedLabels = res;
        this.end();
      },
      error: () => this.fail('Unable to suggest labels.'),
    });
  }

  runSuggestFollowUp(): void {
    if (!this.conversationId) return;
    this.begin();
    this.captain.suggestFollowUp(this.accountId, this.conversationId).subscribe({
      next: (res) => {
        this.followUps = res;
        this.end();
      },
      error: () => this.fail('Unable to suggest follow-ups.'),
    });
  }

  copyToClipboard(text: string): void {
    navigator.clipboard?.writeText(text).catch(() => { /* ignore */ });
  }

  private begin(): void {
    this.state = { loading: true, error: null };
  }

  private end(): void {
    this.state = { loading: false, error: null };
  }

  private fail(msg: string): void {
    this.state = { loading: false, error: msg };
  }

  private resetResults(): void {
    this.suggestedReply = '';
    this.summary = null;
    this.rewriteResult = null;
    this.suggestedLabels = [];
    this.followUps = [];
    this.state = { loading: false, error: null };
  }
}
