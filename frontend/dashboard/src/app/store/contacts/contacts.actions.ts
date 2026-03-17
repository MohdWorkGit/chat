import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Contact } from '@core/models/contact.model';
import { ApiError, PaginatedResult } from '@core/models/common.model';
import { ContactFilters } from '@core/services/contact.service';

export const ContactsActions = createActionGroup({
  source: 'Contacts',
  events: {
    'Load Contacts': props<{ filters?: ContactFilters }>(),
    'Load Contacts Success': props<{ result: PaginatedResult<Contact> }>(),
    'Load Contacts Failure': props<{ error: ApiError }>(),

    'Load Contact': props<{ id: number }>(),
    'Load Contact Success': props<{ contact: Contact }>(),
    'Load Contact Failure': props<{ error: ApiError }>(),

    'Create Contact': props<{ data: Partial<Contact> }>(),
    'Create Contact Success': props<{ contact: Contact }>(),
    'Create Contact Failure': props<{ error: ApiError }>(),

    'Update Contact': props<{ id: number; data: Partial<Contact> }>(),
    'Update Contact Success': props<{ contact: Contact }>(),
    'Update Contact Failure': props<{ error: ApiError }>(),

    'Delete Contact': props<{ id: number }>(),
    'Delete Contact Success': props<{ id: number }>(),
    'Delete Contact Failure': props<{ error: ApiError }>(),

    'Select Contact': props<{ id: number | null }>(),

    'Search Contacts': props<{ query: string }>(),
    'Search Contacts Success': props<{ result: PaginatedResult<Contact> }>(),
    'Search Contacts Failure': props<{ error: ApiError }>(),

    'Clear Error': emptyProps(),
  },
});
