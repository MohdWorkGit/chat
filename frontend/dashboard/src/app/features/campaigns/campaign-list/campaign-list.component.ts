import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { CampaignsActions } from '@store/campaigns/campaigns.actions';
import { selectAllCampaigns, selectCampaignsLoading } from '@store/campaigns/campaigns.selectors';
import { Campaign } from '@core/models/campaign.model';

@Component({
  selector: 'app-campaign-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="p-6">
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-lg font-semibold text-gray-900">Campaigns</h2>
        <button
          (click)="showCreateForm = !showCreateForm"
          class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors"
        >
          {{ showCreateForm ? 'Cancel' : 'New Campaign' }}
        </button>
      </div>

      @if (showCreateForm) {
        <div class="bg-white rounded-lg border border-gray-200 p-6 mb-6">
          <h3 class="text-sm font-semibold text-gray-900 mb-4">Create Campaign</h3>
          <form [formGroup]="createForm" (ngSubmit)="createCampaign()">
            <div class="grid grid-cols-2 gap-4 mb-4">
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">Title</label>
                <input
                  formControlName="title"
                  class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                  placeholder="Campaign title"
                />
              </div>
              <div>
                <label class="block text-xs font-medium text-gray-700 mb-1">Type</label>
                <select
                  formControlName="campaignType"
                  class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                >
                  <option value="ongoing">Ongoing</option>
                  <option value="one_off">One-off</option>
                </select>
              </div>
            </div>
            <div class="mb-4">
              <label class="block text-xs font-medium text-gray-700 mb-1">Description</label>
              <textarea
                formControlName="description"
                rows="2"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="Campaign description"
              ></textarea>
            </div>
            <div class="mb-4">
              <label class="block text-xs font-medium text-gray-700 mb-1">Message</label>
              <textarea
                formControlName="message"
                rows="3"
                class="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="Message to send"
              ></textarea>
            </div>
            <div class="flex justify-end">
              <button
                type="submit"
                [disabled]="createForm.invalid"
                class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg disabled:opacity-50 transition-colors"
              >
                Create
              </button>
            </div>
          </form>
        </div>
      }

      @if (loading$ | async) {
        <div class="flex items-center justify-center py-12">
          <div class="h-8 w-8 animate-spin rounded-full border-4 border-blue-600 border-t-transparent"></div>
        </div>
      } @else {
        @if ((campaigns$ | async); as campaigns) {
          <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
            @if (campaigns.length > 0) {
              <ul class="divide-y divide-gray-200">
                @for (campaign of campaigns; track campaign.id) {
                  <li class="px-6 py-4 hover:bg-gray-50 transition-colors">
                    <div class="flex items-center justify-between">
                      <div class="flex-1">
                        <div class="flex items-center gap-3">
                          <span class="text-sm font-medium text-gray-900">{{ campaign.title }}</span>
                          <span
                            class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium"
                            [class]="getStatusClass(campaign.status)"
                          >
                            {{ campaign.status }}
                          </span>
                          <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-600">
                            {{ campaign.campaignType === 'one_off' ? 'One-off' : 'Ongoing' }}
                          </span>
                        </div>
                        @if (campaign.description) {
                          <p class="text-xs text-gray-500 mt-1">{{ campaign.description }}</p>
                        }
                      </div>
                      <div class="flex items-center gap-3">
                        <button
                          (click)="toggleEnabled(campaign)"
                          class="text-sm"
                          [class]="campaign.enabled ? 'text-yellow-600 hover:text-yellow-800' : 'text-green-600 hover:text-green-800'"
                        >
                          {{ campaign.enabled ? 'Disable' : 'Enable' }}
                        </button>
                        <button
                          (click)="deleteCampaign(campaign.id)"
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
                <p class="text-sm text-gray-500">No campaigns created yet.</p>
                <p class="text-xs text-gray-400 mt-1">Create a campaign to proactively engage customers.</p>
              </div>
            }
          </div>
        }
      }
    </div>
  `,
  styles: [`:host { display: block; height: 100%; }`],
})
export class CampaignListComponent implements OnInit {
  private store = inject(Store);
  private fb = inject(FormBuilder);

  campaigns$ = this.store.select(selectAllCampaigns);
  loading$ = this.store.select(selectCampaignsLoading);

  showCreateForm = false;

  createForm: FormGroup = this.fb.group({
    title: ['', Validators.required],
    campaignType: ['ongoing', Validators.required],
    description: [''],
    message: [''],
  });

  ngOnInit(): void {
    this.store.dispatch(CampaignsActions.loadCampaigns());
  }

  createCampaign(): void {
    if (this.createForm.invalid) return;
    this.store.dispatch(
      CampaignsActions.createCampaign({
        data: {
          ...this.createForm.value,
          enabled: true,
          status: 'draft',
        },
      })
    );
    this.createForm.reset({ campaignType: 'ongoing' });
    this.showCreateForm = false;
  }

  toggleEnabled(campaign: Campaign): void {
    this.store.dispatch(
      CampaignsActions.updateCampaign({
        id: campaign.id,
        data: { enabled: !campaign.enabled },
      })
    );
  }

  deleteCampaign(id: number): void {
    if (confirm('Are you sure you want to delete this campaign?')) {
      this.store.dispatch(CampaignsActions.deleteCampaign({ id }));
    }
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'active': return 'bg-green-100 text-green-800';
      case 'completed': return 'bg-blue-100 text-blue-800';
      default: return 'bg-yellow-100 text-yellow-800';
    }
  }
}
