import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of, switchMap } from 'rxjs';
import { CaptainService } from '@core/services/captain.service';
import { CaptainActions } from './captain.actions';
import { ApiError } from '@core/models/common.model';

export const loadAssistants$ = createEffect(
  (actions$ = inject(Actions), svc = inject(CaptainService)) =>
    actions$.pipe(
      ofType(CaptainActions.loadAssistants),
      switchMap(({ accountId }) =>
        svc.getAssistants(accountId).pipe(
          map((assistants) => CaptainActions.loadAssistantsSuccess({ assistants })),
          catchError((error: ApiError) => of(CaptainActions.loadAssistantsFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const createAssistant$ = createEffect(
  (actions$ = inject(Actions), svc = inject(CaptainService)) =>
    actions$.pipe(
      ofType(CaptainActions.createAssistant),
      exhaustMap(({ accountId, data }) =>
        svc.createAssistant(accountId, data).pipe(
          map((assistant) => CaptainActions.createAssistantSuccess({ assistant })),
          catchError((error: ApiError) => of(CaptainActions.createAssistantFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const updateAssistant$ = createEffect(
  (actions$ = inject(Actions), svc = inject(CaptainService)) =>
    actions$.pipe(
      ofType(CaptainActions.updateAssistant),
      exhaustMap(({ accountId, id, data }) =>
        svc.updateAssistant(accountId, id, data).pipe(
          map((assistant) => CaptainActions.updateAssistantSuccess({ assistant })),
          catchError((error: ApiError) => of(CaptainActions.updateAssistantFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const deleteAssistant$ = createEffect(
  (actions$ = inject(Actions), svc = inject(CaptainService)) =>
    actions$.pipe(
      ofType(CaptainActions.deleteAssistant),
      exhaustMap(({ accountId, id }) =>
        svc.deleteAssistant(accountId, id).pipe(
          map(() => CaptainActions.deleteAssistantSuccess({ id })),
          catchError((error: ApiError) => of(CaptainActions.deleteAssistantFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const loadDocuments$ = createEffect(
  (actions$ = inject(Actions), svc = inject(CaptainService)) =>
    actions$.pipe(
      ofType(CaptainActions.loadDocuments),
      switchMap(({ accountId, assistantId }) =>
        svc.getDocuments(accountId, assistantId).pipe(
          map((documents) => CaptainActions.loadDocumentsSuccess({ documents })),
          catchError((error: ApiError) => of(CaptainActions.loadDocumentsFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const uploadDocument$ = createEffect(
  (actions$ = inject(Actions), svc = inject(CaptainService)) =>
    actions$.pipe(
      ofType(CaptainActions.uploadDocument),
      exhaustMap(({ accountId, assistantId, file }) =>
        svc.uploadDocument(accountId, assistantId, file).pipe(
          map((document) => CaptainActions.uploadDocumentSuccess({ document })),
          catchError((error: ApiError) => of(CaptainActions.uploadDocumentFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const deleteDocument$ = createEffect(
  (actions$ = inject(Actions), svc = inject(CaptainService)) =>
    actions$.pipe(
      ofType(CaptainActions.deleteDocument),
      exhaustMap(({ accountId, assistantId, docId }) =>
        svc.deleteDocument(accountId, assistantId, docId).pipe(
          map(() => CaptainActions.deleteDocumentSuccess({ docId })),
          catchError((error: ApiError) => of(CaptainActions.deleteDocumentFailure({ error })))
        )
      )
    ),
  { functional: true }
);
