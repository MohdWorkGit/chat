import { createFeatureSelector, createSelector } from '@ngrx/store';
import { AuthState } from './auth.reducer';

export const selectAuthState = createFeatureSelector<AuthState>('auth');

export const selectCurrentUser = createSelector(selectAuthState, (state) => state.user);

export const selectIsAuthenticated = createSelector(selectAuthState, (state) => state.isAuthenticated);

export const selectAuthLoading = createSelector(selectAuthState, (state) => state.loading);

export const selectAuthError = createSelector(selectAuthState, (state) => state.error);

export const selectForgotPasswordSuccess = createSelector(selectAuthState, (state) => state.forgotPasswordSuccess);

export const selectUserRole = createSelector(selectCurrentUser, (user) => user?.role);

export const selectUserAvailability = createSelector(selectCurrentUser, (user) => user?.availability);
