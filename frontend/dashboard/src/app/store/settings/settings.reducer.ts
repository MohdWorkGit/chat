import { createReducer, on } from '@ngrx/store';
import { AccountSettings, UserProfile, NotificationPreferences } from '@core/models/settings.model';
import { ApiError } from '@core/models/common.model';
import { SettingsActions } from './settings.actions';

export interface SettingsState {
  accountSettings: AccountSettings | null;
  profile: UserProfile | null;
  notificationPreferences: NotificationPreferences | null;
  loading: boolean;
  error: ApiError | null;
}

export const initialSettingsState: SettingsState = {
  accountSettings: null,
  profile: null,
  notificationPreferences: null,
  loading: false,
  error: null,
};

export const settingsReducer = createReducer(
  initialSettingsState,

  on(SettingsActions.loadAccountSettings, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(SettingsActions.loadAccountSettingsSuccess, (state, { accountSettings }) => ({
    ...state,
    accountSettings,
    loading: false,
  })),

  on(SettingsActions.loadAccountSettingsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(SettingsActions.updateAccountSettings, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(SettingsActions.updateAccountSettingsSuccess, (state, { accountSettings }) => ({
    ...state,
    accountSettings,
    loading: false,
  })),

  on(SettingsActions.updateAccountSettingsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(SettingsActions.loadProfile, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(SettingsActions.loadProfileSuccess, (state, { profile }) => ({
    ...state,
    profile,
    loading: false,
  })),

  on(SettingsActions.loadProfileFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(SettingsActions.updateProfile, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(SettingsActions.updateProfileSuccess, (state, { profile }) => ({
    ...state,
    profile,
    loading: false,
  })),

  on(SettingsActions.updateProfileFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(SettingsActions.updateNotificationPreferences, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(SettingsActions.updateNotificationPreferencesSuccess, (state, { prefs }) => ({
    ...state,
    notificationPreferences: prefs,
    loading: false,
  })),

  on(SettingsActions.updateNotificationPreferencesFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(SettingsActions.clearError, (state) => ({
    ...state,
    error: null,
  }))
);
