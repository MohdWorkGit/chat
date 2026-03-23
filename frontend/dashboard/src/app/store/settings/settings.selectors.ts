import { createFeatureSelector, createSelector } from '@ngrx/store';
import { SettingsState } from './settings.reducer';

export const selectSettingsState = createFeatureSelector<SettingsState>('settings');

export const selectAccountSettings = createSelector(selectSettingsState, (state) => state.accountSettings);

export const selectProfile = createSelector(selectSettingsState, (state) => state.profile);

export const selectNotificationPreferences = createSelector(selectSettingsState, (state) => state.notificationPreferences);

export const selectSettingsLoading = createSelector(selectSettingsState, (state) => state.loading);

export const selectSettingsError = createSelector(selectSettingsState, (state) => state.error);
