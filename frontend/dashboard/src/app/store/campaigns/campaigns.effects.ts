import { inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, exhaustMap, map, of, switchMap } from 'rxjs';
import { CampaignService } from '@core/services/campaign.service';
import { CampaignsActions } from './campaigns.actions';
import { ApiError } from '@core/models/common.model';

export const loadCampaigns$ = createEffect(
  (actions$ = inject(Actions), svc = inject(CampaignService)) =>
    actions$.pipe(
      ofType(CampaignsActions.loadCampaigns),
      switchMap(() =>
        svc.getAll().pipe(
          map((campaigns) => CampaignsActions.loadCampaignsSuccess({ campaigns })),
          catchError((error: ApiError) => of(CampaignsActions.loadCampaignsFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const createCampaign$ = createEffect(
  (actions$ = inject(Actions), svc = inject(CampaignService)) =>
    actions$.pipe(
      ofType(CampaignsActions.createCampaign),
      exhaustMap(({ data }) =>
        svc.create(data).pipe(
          map((campaign) => CampaignsActions.createCampaignSuccess({ campaign })),
          catchError((error: ApiError) => of(CampaignsActions.createCampaignFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const updateCampaign$ = createEffect(
  (actions$ = inject(Actions), svc = inject(CampaignService)) =>
    actions$.pipe(
      ofType(CampaignsActions.updateCampaign),
      exhaustMap(({ id, data }) =>
        svc.update(id, data).pipe(
          map((campaign) => CampaignsActions.updateCampaignSuccess({ campaign })),
          catchError((error: ApiError) => of(CampaignsActions.updateCampaignFailure({ error })))
        )
      )
    ),
  { functional: true }
);

export const deleteCampaign$ = createEffect(
  (actions$ = inject(Actions), svc = inject(CampaignService)) =>
    actions$.pipe(
      ofType(CampaignsActions.deleteCampaign),
      exhaustMap(({ id }) =>
        svc.delete(id).pipe(
          map(() => CampaignsActions.deleteCampaignSuccess({ id })),
          catchError((error: ApiError) => of(CampaignsActions.deleteCampaignFailure({ error })))
        )
      )
    ),
  { functional: true }
);
