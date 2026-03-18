import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Campaign } from '@core/models/campaign.model';
import { ApiError } from '@core/models/common.model';

export const CampaignsActions = createActionGroup({
  source: 'Campaigns',
  events: {
    'Load Campaigns': emptyProps(),
    'Load Campaigns Success': props<{ campaigns: Campaign[] }>(),
    'Load Campaigns Failure': props<{ error: ApiError }>(),

    'Create Campaign': props<{ data: Partial<Campaign> }>(),
    'Create Campaign Success': props<{ campaign: Campaign }>(),
    'Create Campaign Failure': props<{ error: ApiError }>(),

    'Update Campaign': props<{ id: number; data: Partial<Campaign> }>(),
    'Update Campaign Success': props<{ campaign: Campaign }>(),
    'Update Campaign Failure': props<{ error: ApiError }>(),

    'Delete Campaign': props<{ id: number }>(),
    'Delete Campaign Success': props<{ id: number }>(),
    'Delete Campaign Failure': props<{ error: ApiError }>(),

    'Select Campaign': props<{ id: number | null }>(),
    'Clear Error': emptyProps(),
  },
});
