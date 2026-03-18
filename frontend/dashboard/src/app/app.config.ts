import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { routes } from './app.routes';
import { authReducer } from '@store/auth/auth.reducer';
import { contactsReducer } from '@store/contacts/contacts.reducer';
import { conversationsReducer } from '@store/conversations/conversations.reducer';
import { notificationsReducer } from '@store/notifications/notifications.reducer';
import { inboxesReducer } from '@store/inboxes/inboxes.reducer';
import { teamsReducer } from '@store/teams/teams.reducer';
import { labelsReducer } from '@store/labels/labels.reducer';
import * as authEffects from '@store/auth/auth.effects';
import * as contactsEffects from '@store/contacts/contacts.effects';
import * as conversationsEffects from '@store/conversations/conversations.effects';
import * as notificationsEffects from '@store/notifications/notifications.effects';
import * as inboxesEffects from '@store/inboxes/inboxes.effects';
import * as teamsEffects from '@store/teams/teams.effects';
import * as labelsEffects from '@store/labels/labels.effects';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withFetch()),
    provideStore({
      auth: authReducer,
      contacts: contactsReducer,
      conversations: conversationsReducer,
      notifications: notificationsReducer,
      inboxes: inboxesReducer,
      teams: teamsReducer,
      labels: labelsReducer,
    }),
    provideEffects([
      authEffects,
      contactsEffects,
      conversationsEffects,
      notificationsEffects,
      inboxesEffects,
      teamsEffects,
      labelsEffects,
    ]),
  ]
};
