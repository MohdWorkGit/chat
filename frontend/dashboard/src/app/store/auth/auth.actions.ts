import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { LoginRequest, RegisterRequest, AuthResponse } from '@core/models/auth.model';
import { User } from '@core/models/user.model';
import { ApiError } from '@core/models/common.model';

export const AuthActions = createActionGroup({
  source: 'Auth',
  events: {
    'Login': props<{ credentials: LoginRequest }>(),
    'Login Success': props<{ response: AuthResponse }>(),
    'Login Failure': props<{ error: ApiError }>(),

    'Register': props<{ data: RegisterRequest }>(),
    'Register Success': props<{ response: AuthResponse }>(),
    'Register Failure': props<{ error: ApiError }>(),

    'Logout': emptyProps(),
    'Logout Complete': emptyProps(),

    'Load Current User': emptyProps(),
    'Load Current User Success': props<{ user: User }>(),
    'Load Current User Failure': props<{ error: ApiError }>(),

    'Forgot Password': props<{ email: string }>(),
    'Forgot Password Success': emptyProps(),
    'Forgot Password Failure': props<{ error: ApiError }>(),

    'Update Availability': props<{ availability: string }>(),

    'Clear Error': emptyProps(),
  },
});
