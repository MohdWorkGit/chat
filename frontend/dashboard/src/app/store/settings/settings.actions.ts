import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { AccountSettings, UserProfile, NotificationPreferences } from '@core/models/settings.model';
import { ApiError } from '@core/models/common.model';

export const SettingsActions = createActionGroup({
  source: 'Settings',
  events: {
    'Load Account Settings': emptyProps(),
    'Load Account Settings Success': props<{ accountSettings: AccountSettings }>(),
    'Load Account Settings Failure': props<{ error: ApiError }>(),

    'Update Account Settings': props<{ data: Partial<AccountSettings> }>(),
    'Update Account Settings Success': props<{ accountSettings: AccountSettings }>(),
    'Update Account Settings Failure': props<{ error: ApiError }>(),

    'Load Profile': emptyProps(),
    'Load Profile Success': props<{ profile: UserProfile }>(),
    'Load Profile Failure': props<{ error: ApiError }>(),

    'Update Profile': props<{ data: Partial<UserProfile> }>(),
    'Update Profile Success': props<{ profile: UserProfile }>(),
    'Update Profile Failure': props<{ error: ApiError }>(),

    'Update Notification Preferences': props<{ prefs: NotificationPreferences }>(),
    'Update Notification Preferences Success': props<{ prefs: NotificationPreferences }>(),
    'Update Notification Preferences Failure': props<{ error: ApiError }>(),

    'Clear Error': emptyProps(),
  },
});
