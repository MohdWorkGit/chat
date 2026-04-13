import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of, switchMap, concatMap } from 'rxjs';
import { InboxService } from '@core/services/inbox.service';
import { InboxesActions } from './inboxes.actions';
import { ApiError } from '@core/models/common.model';

export const loadInboxes$ = createEffect(
  (actions$ = inject(Actions), inboxService = inject(InboxService)) =>
    actions$.pipe(
      ofType(InboxesActions.loadInboxes),
      switchMap(() =>
        inboxService.getAll().pipe(
          map((inboxes) => InboxesActions.loadInboxesSuccess({ inboxes })),
          catchError((error: ApiError) => of(InboxesActions.loadInboxesFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const loadInbox$ = createEffect(
  (actions$ = inject(Actions), inboxService = inject(InboxService)) =>
    actions$.pipe(
      ofType(InboxesActions.loadInbox),
      switchMap(({ id }) =>
        inboxService.getById(id).pipe(
          map((inbox) => InboxesActions.loadInboxSuccess({ inbox })),
          catchError((error: ApiError) => of(InboxesActions.loadInboxFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const createInbox$ = createEffect(
  (actions$ = inject(Actions), inboxService = inject(InboxService)) =>
    actions$.pipe(
      ofType(InboxesActions.createInbox),
      exhaustMap(({ data }) =>
        inboxService.create(data).pipe(
          map((inbox) => InboxesActions.createInboxSuccess({ inbox })),
          catchError((error: ApiError) => of(InboxesActions.createInboxFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const updateInbox$ = createEffect(
  (actions$ = inject(Actions), inboxService = inject(InboxService)) =>
    actions$.pipe(
      ofType(InboxesActions.updateInbox),
      exhaustMap(({ id, data }) =>
        inboxService.update(id, data).pipe(
          map((inbox) => InboxesActions.updateInboxSuccess({ inbox })),
          catchError((error: ApiError) => of(InboxesActions.updateInboxFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const deleteInbox$ = createEffect(
  (actions$ = inject(Actions), inboxService = inject(InboxService)) =>
    actions$.pipe(
      ofType(InboxesActions.deleteInbox),
      exhaustMap(({ id }) =>
        inboxService.delete(id).pipe(
          map(() => InboxesActions.deleteInboxSuccess({ id })),
          catchError((error: ApiError) => of(InboxesActions.deleteInboxFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const addMember$ = createEffect(
  (actions$ = inject(Actions), inboxService = inject(InboxService)) =>
    actions$.pipe(
      ofType(InboxesActions.addMember),
      concatMap(({ inboxId, userId }) =>
        inboxService.addMember(inboxId, userId).pipe(
          concatMap(() =>
            inboxService.getById(inboxId).pipe(
              map((inbox) => InboxesActions.addMemberSuccess({ inbox }))
            )
          ),
          catchError((error: ApiError) => of(InboxesActions.addMemberFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const removeMember$ = createEffect(
  (actions$ = inject(Actions), inboxService = inject(InboxService)) =>
    actions$.pipe(
      ofType(InboxesActions.removeMember),
      concatMap(({ inboxId, memberId }) =>
        inboxService.removeMember(inboxId, memberId).pipe(
          concatMap(() =>
            inboxService.getById(inboxId).pipe(
              map((inbox) => InboxesActions.removeMemberSuccess({ inbox }))
            )
          ),
          catchError((error: ApiError) => of(InboxesActions.removeMemberFailure({ error })))
        )
      )
    ),
  { functional: true }
);
