import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of, switchMap } from 'rxjs';
import { HelpCenterService } from '@core/services/helpcenter.service';
import { HelpCenterActions } from './helpcenter.actions';
import { ApiError } from '@core/models/common.model';

export const loadArticles$ = createEffect(
  (actions$ = inject(Actions), helpCenterService = inject(HelpCenterService)) =>
    actions$.pipe(
      ofType(HelpCenterActions.loadArticles),
      switchMap(() =>
        helpCenterService.getArticles().pipe(
          map((articles) => HelpCenterActions.loadArticlesSuccess({ articles })),
          catchError((error: ApiError) => of(HelpCenterActions.loadArticlesFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const loadArticle$ = createEffect(
  (actions$ = inject(Actions), helpCenterService = inject(HelpCenterService)) =>
    actions$.pipe(
      ofType(HelpCenterActions.loadArticle),
      switchMap(({ id }) =>
        helpCenterService.getArticle(id).pipe(
          map((article) => HelpCenterActions.loadArticleSuccess({ article })),
          catchError((error: ApiError) => of(HelpCenterActions.loadArticleFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const createArticle$ = createEffect(
  (actions$ = inject(Actions), helpCenterService = inject(HelpCenterService)) =>
    actions$.pipe(
      ofType(HelpCenterActions.createArticle),
      exhaustMap(({ data }) =>
        helpCenterService.createArticle(data).pipe(
          map((article) => HelpCenterActions.createArticleSuccess({ article })),
          catchError((error: ApiError) => of(HelpCenterActions.createArticleFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const updateArticle$ = createEffect(
  (actions$ = inject(Actions), helpCenterService = inject(HelpCenterService)) =>
    actions$.pipe(
      ofType(HelpCenterActions.updateArticle),
      exhaustMap(({ id, data }) =>
        helpCenterService.updateArticle(id, data).pipe(
          map((article) => HelpCenterActions.updateArticleSuccess({ article })),
          catchError((error: ApiError) => of(HelpCenterActions.updateArticleFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const deleteArticle$ = createEffect(
  (actions$ = inject(Actions), helpCenterService = inject(HelpCenterService)) =>
    actions$.pipe(
      ofType(HelpCenterActions.deleteArticle),
      exhaustMap(({ id }) =>
        helpCenterService.deleteArticle(id).pipe(
          map(() => HelpCenterActions.deleteArticleSuccess({ id })),
          catchError((error: ApiError) => of(HelpCenterActions.deleteArticleFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const loadPortals$ = createEffect(
  (actions$ = inject(Actions), helpCenterService = inject(HelpCenterService)) =>
    actions$.pipe(
      ofType(HelpCenterActions.loadPortals),
      switchMap(() =>
        helpCenterService.getPortals().pipe(
          map((portals) => HelpCenterActions.loadPortalsSuccess({ portals })),
          catchError((error: ApiError) => of(HelpCenterActions.loadPortalsFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const loadCategories$ = createEffect(
  (actions$ = inject(Actions), helpCenterService = inject(HelpCenterService)) =>
    actions$.pipe(
      ofType(HelpCenterActions.loadCategories),
      switchMap(({ portalId }) =>
        helpCenterService.getCategories(portalId).pipe(
          map((categories) => HelpCenterActions.loadCategoriesSuccess({ categories })),
          catchError((error: ApiError) => of(HelpCenterActions.loadCategoriesFailure({ error })))
        )
      )
    ),
  { functional: true }
);
