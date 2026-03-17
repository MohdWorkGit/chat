import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of, switchMap } from 'rxjs';
import { ContactService } from '@core/services/contact.service';
import { ContactsActions } from './contacts.actions';
import { ApiError } from '@core/models/common.model';

export const loadContacts$ = createEffect(
  (actions$ = inject(Actions), contactService = inject(ContactService)) =>
    actions$.pipe(
      ofType(ContactsActions.loadContacts),
      switchMap(({ filters }) =>
        contactService.getAll(filters ?? {}).pipe(
          map((result) => ContactsActions.loadContactsSuccess({ result })),
          catchError((error: ApiError) => of(ContactsActions.loadContactsFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const loadContact$ = createEffect(
  (actions$ = inject(Actions), contactService = inject(ContactService)) =>
    actions$.pipe(
      ofType(ContactsActions.loadContact),
      switchMap(({ id }) =>
        contactService.getById(id).pipe(
          map((contact) => ContactsActions.loadContactSuccess({ contact })),
          catchError((error: ApiError) => of(ContactsActions.loadContactFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const createContact$ = createEffect(
  (actions$ = inject(Actions), contactService = inject(ContactService)) =>
    actions$.pipe(
      ofType(ContactsActions.createContact),
      exhaustMap(({ data }) =>
        contactService.create(data).pipe(
          map((contact) => ContactsActions.createContactSuccess({ contact })),
          catchError((error: ApiError) => of(ContactsActions.createContactFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const updateContact$ = createEffect(
  (actions$ = inject(Actions), contactService = inject(ContactService)) =>
    actions$.pipe(
      ofType(ContactsActions.updateContact),
      exhaustMap(({ id, data }) =>
        contactService.update(id, data).pipe(
          map((contact) => ContactsActions.updateContactSuccess({ contact })),
          catchError((error: ApiError) => of(ContactsActions.updateContactFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const deleteContact$ = createEffect(
  (actions$ = inject(Actions), contactService = inject(ContactService)) =>
    actions$.pipe(
      ofType(ContactsActions.deleteContact),
      exhaustMap(({ id }) =>
        contactService.delete(id).pipe(
          map(() => ContactsActions.deleteContactSuccess({ id })),
          catchError((error: ApiError) => of(ContactsActions.deleteContactFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const searchContacts$ = createEffect(
  (actions$ = inject(Actions), contactService = inject(ContactService)) =>
    actions$.pipe(
      ofType(ContactsActions.searchContacts),
      switchMap(({ query }) =>
        contactService.search(query).pipe(
          map((result) => ContactsActions.searchContactsSuccess({ result })),
          catchError((error: ApiError) => of(ContactsActions.searchContactsFailure({ error })))
        )
      )
    ),
  { functional: true }
);
