import { createFeatureSelector, createSelector } from '@ngrx/store';
import { CampaignsState, campaignsAdapter } from './campaigns.reducer';

export const selectCampaignsState = createFeatureSelector<CampaignsState>('campaigns');

const { selectAll, selectEntities } = campaignsAdapter.getSelectors();

export const selectAllCampaigns = createSelector(selectCampaignsState, selectAll);
export const selectCampaignEntities = createSelector(selectCampaignsState, selectEntities);
export const selectSelectedCampaignId = createSelector(selectCampaignsState, (state) => state.selectedCampaignId);
export const selectSelectedCampaign = createSelector(
  selectCampaignEntities,
  selectSelectedCampaignId,
  (entities, selectedId) => (selectedId !== null ? entities[selectedId] ?? null : null)
);
export const selectCampaignsLoading = createSelector(selectCampaignsState, (state) => state.loading);
export const selectCampaignsError = createSelector(selectCampaignsState, (state) => state.error);
