import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { EMPTY, catchError, exhaustMap, map, mergeMap, of, switchMap } from 'rxjs';
import { ConversationService } from '@core/services/conversation.service';
import { SignalRService } from '@core/services/signalr.service';
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

// Listen for real-time `message.created` events from SignalR and push them
// into the store so the conversation view updates without a manual refresh.
export const messageReceivedFromSignalR$ = createEffect(
  (signalrService = inject(SignalRService)) =>
    signalrService.messageCreated$.pipe(
      map((message) => ConversationsActions.messageReceived({ message }))
    ),
  { functional: true }
);

// Listen for real-time `conversation.created` events from SignalR and pull the
// full conversation into the store so the list updates without a manual
// refresh. The broadcast payload only carries identifiers (see
// BroadcastEventHandler.Handle(ConversationCreatedEvent)); the list template
// and the reducer's sortComparer need related data (contact, assignee, inbox,
// lastActivityAt), so we fetch the full entity by id and reuse
// loadConversationSuccess which upserts into the store. `mergeMap` is used
// instead of `switchMap` so rapid bursts of new conversations are not
// cancelled mid-flight.
export const snoozeConversation$ = createEffect(
  (actions$ = inject(Actions), conversationService = inject(ConversationService)) =>
    actions$.pipe(
      ofType(ConversationsActions.snoozeConversation),
      exhaustMap(({ id, snoozeUntil }) =>
        conversationService.snooze(id, snoozeUntil).pipe(
          map(() => ConversationsActions.snoozeConversationSuccess({ id, snoozeUntil })),
          catchError((error: ApiError) => of(ConversationsActions.snoozeConversationFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const muteConversation$ = createEffect(
  (actions$ = inject(Actions), conversationService = inject(ConversationService)) =>
    actions$.pipe(
      ofType(ConversationsActions.muteConversation),
      exhaustMap(({ id }) =>
        conversationService.mute(id).pipe(
          map(() => ConversationsActions.muteConversationSuccess({ id })),
          catchError((error: ApiError) => of(ConversationsActions.muteConversationFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const unmuteConversation$ = createEffect(
  (actions$ = inject(Actions), conversationService = inject(ConversationService)) =>
    actions$.pipe(
      ofType(ConversationsActions.unmuteConversation),
      exhaustMap(({ id }) =>
        conversationService.unmute(id).pipe(
          map(() => ConversationsActions.unmuteConversationSuccess({ id })),
          catchError((error: ApiError) => of(ConversationsActions.unmuteConversationFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const togglePriority$ = createEffect(
  (actions$ = inject(Actions), conversationService = inject(ConversationService)) =>
    actions$.pipe(
      ofType(ConversationsActions.togglePriority),
      exhaustMap(({ id }) =>
        conversationService.togglePriority(id).pipe(
          map(() => ConversationsActions.togglePrioritySuccess({ id })),
          catchError((error: ApiError) => of(ConversationsActions.togglePriorityFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const conversationCreatedFromSignalR$ = createEffect(
  (signalrService = inject(SignalRService), conversationService = inject(ConversationService)) =>
    signalrService.conversationCreated$.pipe(
      mergeMap((data) => {
        const rawId = (data as { id?: number | string }).id;
        const id = typeof rawId === 'number' ? rawId : Number(rawId);
        if (!Number.isFinite(id) || id <= 0) {
          return EMPTY;
        }
        return conversationService.getById(id).pipe(
          map((conversation) => ConversationsActions.loadConversationSuccess({ conversation })),
          catchError(() => EMPTY)
        );
      })
    ),
  { functional: true }
);
