import { createFeatureSelector, createSelector } from '@ngrx/store';
import { HelpCenterState, helpCenterAdapter } from './helpcenter.reducer';

export const selectHelpCenterState = createFeatureSelector<HelpCenterState>('helpCenter');

const { selectAll, selectEntities } = helpCenterAdapter.getSelectors();

export const selectAllArticles = createSelector(selectHelpCenterState, selectAll);

export const selectArticleEntities = createSelector(selectHelpCenterState, selectEntities);

export const selectSelectedArticleId = createSelector(selectHelpCenterState, (state) => state.selectedArticleId);

export const selectSelectedArticle = createSelector(
  selectArticleEntities,
  selectSelectedArticleId,
  (entities, selectedId) => (selectedId !== null ? entities[selectedId] ?? null : null)
);

export const selectPortals = createSelector(selectHelpCenterState, (state) => state.portals);

export const selectCategories = createSelector(selectHelpCenterState, (state) => state.categories);

export const selectHelpCenterLoading = createSelector(selectHelpCenterState, (state) => state.loading);

export const selectHelpCenterError = createSelector(selectHelpCenterState, (state) => state.error);
