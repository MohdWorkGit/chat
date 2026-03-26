import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of, switchMap } from 'rxjs';
import { ConversationService } from '@core/services/conversation.service';
import { ConversationsActions } from './conversations.actions';
import { ApiError } from '@core/models/common.model';

export const loadConversations$ = createEffect(
  (actions$ = inject(Actions), conversationService = inject(ConversationService)) =>
    actions$.pipe(
      ofType(ConversationsActions.loadConversations),
      switchMap(({ filters }) =>
        conversationService.getAll(filters ?? {}).pipe(
          map((result) => ConversationsActions.loadConversationsSuccess({ result })),
          catchError((error: ApiError) => of(ConversationsActions.loadConversationsFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const loadConversation$ = createEffect(
  (actions$ = inject(Actions), conversationService = inject(ConversationService)) =>
    actions$.pipe(
      ofType(ConversationsActions.loadConversation),
      switchMap(({ id }) =>
        conversationService.getById(id).pipe(
          map((conversation) => ConversationsActions.loadConversationSuccess({ conversation })),
          catchError((error: ApiError) => of(ConversationsActions.loadConversationFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const updateConversationStatus$ = createEffect(
  (actions$ = inject(Actions), conversationService = inject(ConversationService)) =>
    actions$.pipe(
      ofType(ConversationsActions.updateConversationStatus),
      exhaustMap(({ id, status }) =>
        conversationService.updateStatus(id, status).pipe(
          map((conversation) => ConversationsActions.updateConversationStatusSuccess({ conversation })),
          catchError((error: ApiError) => of(ConversationsActions.updateConversationStatusFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const assignConversation$ = createEffect(
  (actions$ = inject(Actions), conversationService = inject(ConversationService)) =>
    actions$.pipe(
      ofType(ConversationsActions.assignConversation),
      exhaustMap(({ id, assigneeId }) =>
        conversationService.assign(id, assigneeId).pipe(
          map((conversation) => ConversationsActions.assignConversationSuccess({ conversation })),
          catchError((error: ApiError) => of(ConversationsActions.assignConversationFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const loadMessages$ = createEffect(
  (actions$ = inject(Actions), conversationService = inject(ConversationService)) =>
    actions$.pipe(
      ofType(ConversationsActions.loadMessages),
      switchMap(({ conversationId, page }) =>
        conversationService.getMessages(conversationId, page).pipe(
          map((result) => ConversationsActions.loadMessagesSuccess({ conversationId, result })),
          catchError((error: ApiError) => of(ConversationsActions.loadMessagesFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const createConversation$ = createEffect(
  (actions$ = inject(Actions), conversationService = inject(ConversationService)) =>
    actions$.pipe(
      ofType(ConversationsActions.createConversation),
      exhaustMap(({ data }) =>
        conversationService.create(data).pipe(
          map((conversation) => ConversationsActions.createConversationSuccess({ conversation })),
          catchError((error: ApiError) => of(ConversationsActions.createConversationFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const sendMessage$ = createEffect(
  (actions$ = inject(Actions), conversationService = inject(ConversationService)) =>
    actions$.pipe(
      ofType(ConversationsActions.sendMessage),
      exhaustMap(({ conversationId, content, isPrivate }) =>
        conversationService.sendMessage(conversationId, content, isPrivate).pipe(
          map((message) => ConversationsActions.sendMessageSuccess({ message })),
          catchError((error: ApiError) => of(ConversationsActions.sendMessageFailure({ error })))
        )
      )
    ),
  { functional: true }
);
