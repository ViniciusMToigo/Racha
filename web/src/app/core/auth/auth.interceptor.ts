import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { APP_CONFIG } from '../config/app-config';
import { AuthService } from './auth.service';

/**
 * Adds the access token to requests for our own APIs (never to third-party URLs),
 * and sends the user back to the login page if the API rejects the token.
 */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const config = inject(APP_CONFIG);

  const token = auth.accessToken();
  const isOurApi =
    request.url.startsWith(config.apiUrl) || request.url.startsWith(config.authApiUrl);

  const authorized =
    token && isOurApi
      ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : request;

  return next(authorized).pipe(
    catchError((error: unknown) => {
      // Only a rejected token ends the session; a 401 from a failed login is just a wrong password.
      if (error instanceof HttpErrorResponse && error.status === 401 && token) {
        auth.logout();
        void router.navigate(['/login'], { queryParams: { returnUrl: router.url } });
      }
      return throwError(() => error);
    }),
  );
};
