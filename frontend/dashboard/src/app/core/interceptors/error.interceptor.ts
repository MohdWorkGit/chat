import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { ApiError } from '@core/models/common.model';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const apiError: ApiError = {
        status: error.status,
        message: getErrorMessage(error),
        errors: error.error?.errors,
        traceId: error.error?.traceId,
      };

      if (error.status === 0) {
        apiError.message = 'Unable to connect to the server. Please check your network connection.';
      }

      if (error.status >= 500) {
        console.error('Server error:', apiError);
      }

      return throwError(() => apiError);
    })
  );
};

function getErrorMessage(error: HttpErrorResponse): string {
  if (error.error?.message) {
    return error.error.message;
  }

  switch (error.status) {
    case 400:
      return 'Bad request. Please check your input.';
    case 401:
      return 'Unauthorized. Please log in again.';
    case 403:
      return 'You do not have permission to perform this action.';
    case 404:
      return 'The requested resource was not found.';
    case 409:
      return 'A conflict occurred. Please try again.';
    case 422:
      return 'Validation failed. Please check your input.';
    case 429:
      return 'Too many requests. Please wait and try again.';
    default:
      return 'An unexpected error occurred. Please try again later.';
  }
}
