import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  if (!req.url.startsWith(environment.apiUrl)) {
    return next(req);
  }

  const user = authService.currentUser;
  if (user?.accessToken) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${user.accessToken}` }
    });
  }
  return next(req);
};
