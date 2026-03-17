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
import * as authEffects from '@store/auth/auth.effects';
import * as contactsEffects from '@store/contacts/contacts.effects';
import * as conversationsEffects from '@store/conversations/conversations.effects';
import * as notificationsEffects from '@store/notifications/notifications.effects';

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
    }),
    provideEffects([
      authEffects,
      contactsEffects,
      conversationsEffects,
      notificationsEffects,
    ]),
  ]
};
