import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError(error => {
      if (error.status === 401) {
        authService.logout();
      } else if (error.status === 403) {
        snackBar.open('You do not have permission to perform this action.', 'Close', { duration: 5000 });
      } else if (error.status === 429) {
        snackBar.open('Too many requests. Please slow down.', 'Close', { duration: 5000 });
      } else if (error.status >= 500) {
        snackBar.open('A server error occurred. Please try again later.', 'Close', { duration: 5000 });
      }
      return throwError(() => error);
    })
  );
};
