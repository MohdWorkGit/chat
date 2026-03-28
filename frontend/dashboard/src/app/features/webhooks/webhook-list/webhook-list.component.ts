import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { WebhooksActions } from '@store/webhooks/webhooks.actions';
import { selectAllWebhooks, selectWebhooksLoading } from '@store/webhooks/webhooks.selectors';
import { Webhook, WEBHOOK_EVENTS } from '@core/models/webhook.model';

@Component({
  selector: 'app-webhook-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <div>
          <h2 class="text-lg font-semibold text-gray-900">Webhooks</h2>
          <p class="text-sm text-gray-500 mt-1">Configure webhook endpoints to receive real-time event notifications.</p>
        </div>
        <button
          (click)="showCreateForm = !showCreateForm"
          class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
        >
          {{ showCreateForm ? 'Cancel' : 'Add Webhook' }}
        </button>
      </div>

      @if (showCreateForm) {
        <div class="bg-white rounded-lg border border-gray-200 p-6 mb-6">
          <h3 class="text-sm font-medium text-gray-900 mb-4">New Webhook</h3>
          <form [formGroup]="createForm" (ngSubmit)="createWebhook()">
            <div class="space-y-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-1">Endpoint URL</label>
                <input
                  formControlName="url"
                  type="url"
                  class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="https://example.com/webhook"
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">Events</label>
                <div class="grid grid-cols-2 gap-2">
                  @for (event of webhookEvents; track event) {
                    <label class="flex items-center gap-2 text-sm text-gray-700">
                      <input
                        type="checkbox"
                        [checked]="selectedEvents.has(event)"
                        (change)="toggleEvent(event)"
                        class="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                      />
                      {{ event }}
                    </label>
                  }
                </div>
              </div>
              <div class="flex justify-end">
                <button
                  type="submit"
                  [disabled]="createForm.invalid || selectedEvents.size === 0"
                  class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 transition-colors"
                >
                  Create Webhook
                </button>
              </div>
            </div>
          </form>
        </div>
      }

      @if (loading$ | async) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if ((webhooks$ | async); as webhooks) {
          <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
            @if (webhooks.length > 0) {
              <ul class="divide-y divide-gray-200">
                @for (webhook of webhooks; track webhook.id) {
                  <li class="px-6 py-4 hover:bg-gray-50 transition-colors">
                    <div class="flex items-center justify-between">
                      <div class="flex-1 min-w-0">
                        <p class="text-sm font-medium text-gray-900 truncate">{{ webhook.url }}</p>
                        <div class="flex flex-wrap gap-1 mt-1">
                          @for (sub of webhook.subscriptions; track sub) {
                            <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-blue-100 text-blue-800">
                              {{ sub }}
                            </span>
                          }
                        </div>
                      </div>
                      <div class="flex items-center gap-3 ml-4">
                        <button
                          (click)="deleteWebhook(webhook.id)"
                          class="text-sm text-red-600 hover:text-red-800"
                        >
                          Delete
                        </button>
                      </div>
                    </div>
                  </li>
                }
              </ul>
            } @else {
              <div class="text-center py-12">
                <p class="text-sm text-gray-500">No webhooks configured yet.</p>
                <p class="text-xs text-gray-400 mt-1">Add a webhook to start receiving event notifications.</p>
              </div>
            }
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
export class WebhookListComponent implements OnInit {
  private store = inject(Store);
  private fb = inject(FormBuilder);

  webhooks$ = this.store.select(selectAllWebhooks);
  loading$ = this.store.select(selectWebhooksLoading);

  webhookEvents = [...WEBHOOK_EVENTS];
  selectedEvents = new Set<string>();
  showCreateForm = false;

  createForm: FormGroup = this.fb.group({
    url: ['', [Validators.required]],
  });

  ngOnInit(): void {
    this.store.dispatch(WebhooksActions.loadWebhooks());
  }

  toggleEvent(event: string): void {
    if (this.selectedEvents.has(event)) {
      this.selectedEvents.delete(event);
    } else {
      this.selectedEvents.add(event);
    }
  }

  createWebhook(): void {
    if (this.createForm.invalid || this.selectedEvents.size === 0) return;
    this.store.dispatch(
      WebhooksActions.createWebhook({
        data: {
          url: this.createForm.value.url,
          subscriptions: [...this.selectedEvents],
        },
      })
    );
    this.createForm.reset();
    this.selectedEvents.clear();
    this.showCreateForm = false;
  }

  deleteWebhook(id: number): void {
    if (confirm('Are you sure you want to delete this webhook?')) {
      this.store.dispatch(WebhooksActions.deleteWebhook({ id }));
    }
  }
}
