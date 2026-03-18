import { createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { Campaign } from '@core/models/campaign.model';
import { ApiError } from '@core/models/common.model';
import { CampaignsActions } from './campaigns.actions';

export interface CampaignsState extends EntityState<Campaign> {
  selectedCampaignId: number | null;
  loading: boolean;
  error: ApiError | null;
}

export const campaignsAdapter: EntityAdapter<Campaign> = createEntityAdapter<Campaign>({
  selectId: (campaign) => campaign.id,
  sortComparer: (a, b) => a.title.localeCompare(b.title),
});

export const initialCampaignsState: CampaignsState = campaignsAdapter.getInitialState({
  selectedCampaignId: null,
  loading: false,
  error: null,
});

export const campaignsReducer = createReducer(
  initialCampaignsState,

  on(CampaignsActions.loadCampaigns, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(CampaignsActions.loadCampaignsSuccess, (state, { campaigns }) =>
    campaignsAdapter.setAll(campaigns, { ...state, loading: false })
  ),

  on(CampaignsActions.loadCampaignsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(CampaignsActions.createCampaignSuccess, (state, { campaign }) =>
    campaignsAdapter.addOne(campaign, state)
  ),

  on(CampaignsActions.updateCampaignSuccess, (state, { campaign }) =>
    campaignsAdapter.updateOne({ id: campaign.id, changes: campaign }, state)
  ),

  on(CampaignsActions.deleteCampaignSuccess, (state, { id }) =>
    campaignsAdapter.removeOne(id, state)
  ),

  on(CampaignsActions.selectCampaign, (state, { id }) => ({
    ...state,
    selectedCampaignId: id,
  })),

  on(CampaignsActions.clearError, (state) => ({
    ...state,
    error: null,
  }))
);
