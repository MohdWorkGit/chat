import { createReducer, on } from '@ngrx/store';
import { EntityState, EntityAdapter, createEntityAdapter } from '@ngrx/entity';
import { Inbox } from '@core/models/inbox.model';
import { ApiError } from '@core/models/common.model';
import { InboxesActions } from './inboxes.actions';

export interface InboxesState extends EntityState<Inbox> {
  selectedInboxId: number | null;
  loading: boolean;
  error: ApiError | null;
}

export const inboxesAdapter: EntityAdapter<Inbox> = createEntityAdapter<Inbox>({
  selectId: (inbox) => inbox.id,
  sortComparer: (a, b) => a.name.localeCompare(b.name),
});

export const initialInboxesState: InboxesState = inboxesAdapter.getInitialState({
  selectedInboxId: null,
  loading: false,
  error: null,
});

export const inboxesReducer = createReducer(
  initialInboxesState,

  on(InboxesActions.loadInboxes, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(InboxesActions.loadInboxesSuccess, (state, { inboxes }) =>
    inboxesAdapter.setAll(inboxes, {
      ...state,
      loading: false,
    })
  ),

  on(InboxesActions.loadInboxesFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(InboxesActions.loadInboxSuccess, (state, { inbox }) =>
    inboxesAdapter.upsertOne(inbox, {
      ...state,
      selectedInboxId: inbox.id,
    })
  ),

  on(InboxesActions.createInboxSuccess, (state, { inbox }) =>
    inboxesAdapter.addOne(inbox, state)
  ),

  on(InboxesActions.updateInboxSuccess, (state, { inbox }) =>
    inboxesAdapter.updateOne({ id: inbox.id, changes: inbox }, state)
  ),

  on(InboxesActions.deleteInboxSuccess, (state, { id }) =>
    inboxesAdapter.removeOne(id, state)
  ),

  on(InboxesActions.selectInbox, (state, { id }) => ({
    ...state,
    selectedInboxId: id,
  })),

  on(InboxesActions.addMemberSuccess, (state, { inbox }) =>
    inboxesAdapter.updateOne({ id: inbox.id, changes: inbox }, state)
  ),

  on(InboxesActions.addMemberFailure, (state, { error }) => ({
    ...state,
    error,
  })),

  on(InboxesActions.removeMemberSuccess, (state, { inbox }) =>
    inboxesAdapter.updateOne({ id: inbox.id, changes: inbox }, state)
  ),

  on(InboxesActions.removeMemberFailure, (state, { error }) => ({
    ...state,
    error,
  })),

  on(InboxesActions.clearError, (state) => ({
    ...state,
    error: null,
  }))
);
