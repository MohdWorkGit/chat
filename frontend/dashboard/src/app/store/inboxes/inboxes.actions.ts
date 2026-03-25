import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Inbox } from '@core/models/inbox.model';
import { ApiError } from '@core/models/common.model';

export const InboxesActions = createActionGroup({
  source: 'Inboxes',
  events: {
    'Load Inboxes': emptyProps(),
    'Load Inboxes Success': props<{ inboxes: Inbox[] }>(),
    'Load Inboxes Failure': props<{ error: ApiError }>(),

    'Load Inbox': props<{ id: number }>(),
    'Load Inbox Success': props<{ inbox: Inbox }>(),
    'Load Inbox Failure': props<{ error: ApiError }>(),

    'Create Inbox': props<{ data: Partial<Inbox> }>(),
    'Create Inbox Success': props<{ inbox: Inbox }>(),
    'Create Inbox Failure': props<{ error: ApiError }>(),

    'Update Inbox': props<{ id: number; data: Partial<Inbox> }>(),
    'Update Inbox Success': props<{ inbox: Inbox }>(),
    'Update Inbox Failure': props<{ error: ApiError }>(),

    'Delete Inbox': props<{ id: number }>(),
    'Delete Inbox Success': props<{ id: number }>(),
    'Delete Inbox Failure': props<{ error: ApiError }>(),

    'Select Inbox': props<{ id: number | null }>(),

    'Add Member': props<{ inboxId: number; userId: number }>(),
    'Add Member Success': props<{ inbox: Inbox }>(),
    'Add Member Failure': props<{ error: ApiError }>(),

    'Remove Member': props<{ inboxId: number; memberId: number }>(),
    'Remove Member Success': props<{ inbox: Inbox }>(),
    'Remove Member Failure': props<{ error: ApiError }>(),

    'Clear Error': emptyProps(),
  },
});
