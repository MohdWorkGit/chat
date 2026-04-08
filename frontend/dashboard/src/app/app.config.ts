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
import { automationsReducer } from '@store/automations/automations.reducer';
import { campaignsReducer } from '@store/campaigns/campaigns.reducer';
import { cannedResponsesReducer } from '@store/canned-responses/canned-responses.reducer';
import { macrosReducer } from '@store/macros/macros.reducer';
import * as authEffects from '@store/auth/auth.effects';
import * as contactsEffects from '@store/contacts/contacts.effects';
import * as conversationsEffects from '@store/conversations/conversations.effects';
import * as notificationsEffects from '@store/notifications/notifications.effects';
import * as inboxesEffects from '@store/inboxes/inboxes.effects';
import * as teamsEffects from '@store/teams/teams.effects';
import * as labelsEffects from '@store/labels/labels.effects';
import * as automationsEffects from '@store/automations/automations.effects';
import * as campaignsEffects from '@store/campaigns/campaigns.effects';
import * as cannedResponsesEffects from '@store/canned-responses/canned-responses.effects';
import * as macrosEffects from '@store/macros/macros.effects';
import { customAttributesReducer } from '@store/custom-attributes/custom-attributes.reducer';
import * as customAttributesEffects from '@store/custom-attributes/custom-attributes.effects';
import { webhooksReducer } from '@store/webhooks/webhooks.reducer';
import * as webhooksEffects from '@store/webhooks/webhooks.effects';
import { customFiltersReducer } from '@store/custom-filters/custom-filters.reducer';
import * as customFiltersEffects from '@store/custom-filters/custom-filters.effects';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { jwtInterceptor } from '@core/interceptors/jwt.interceptor';
import { errorInterceptor } from '@core/interceptors/error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withFetch(), withInterceptors([jwtInterceptor, errorInterceptor])),
    provideStore({
      auth: authReducer,
      contacts: contactsReducer,
      conversations: conversationsReducer,
      notifications: notificationsReducer,
      inboxes: inboxesReducer,
      teams: teamsReducer,
      labels: labelsReducer,
      automations: automationsReducer,
      campaigns: campaignsReducer,
      cannedResponses: cannedResponsesReducer,
      macros: macrosReducer,
      customAttributes: customAttributesReducer,
      webhooks: webhooksReducer,
      customFilters: customFiltersReducer,
    }),
    provideEffects([
      authEffects,
      contactsEffects,
      conversationsEffects,
      notificationsEffects,
      inboxesEffects,
      teamsEffects,
      labelsEffects,
      automationsEffects,
      campaignsEffects,
      cannedResponsesEffects,
      macrosEffects,
      customAttributesEffects,
      webhooksEffects,
      customFiltersEffects,
    ]),
  ]
};
