import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { Team } from '@core/models/team.model';
import { ApiError } from '@core/models/common.model';

export const TeamsActions = createActionGroup({
  source: 'Teams',
  events: {
    'Load Teams': emptyProps(),
    'Load Teams Success': props<{ teams: Team[] }>(),
    'Load Teams Failure': props<{ error: ApiError }>(),

    'Load Team': props<{ id: number }>(),
    'Load Team Success': props<{ team: Team }>(),
    'Load Team Failure': props<{ error: ApiError }>(),

    'Create Team': props<{ data: Partial<Team> }>(),
    'Create Team Success': props<{ team: Team }>(),
    'Create Team Failure': props<{ error: ApiError }>(),

    'Update Team': props<{ id: number; data: Partial<Team> }>(),
    'Update Team Success': props<{ team: Team }>(),
    'Update Team Failure': props<{ error: ApiError }>(),

    'Delete Team': props<{ id: number }>(),
    'Delete Team Success': props<{ id: number }>(),
    'Delete Team Failure': props<{ error: ApiError }>(),

    'Select Team': props<{ id: number | null }>(),

    'Clear Error': emptyProps(),
  },
});
