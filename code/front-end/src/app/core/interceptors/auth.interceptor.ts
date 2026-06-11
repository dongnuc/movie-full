import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { catchError, switchMap, throwError, Observable, shareReplay, finalize, map } from 'rxjs';

// Module-level shared refresh token observable to coordinate concurrent 401 challenges
let refreshToken$: Observable<string> | null = null;

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.accessToken();

  let authReq = req;
  if (token) {
    console.log("token", token);
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(authReq).pipe(
    catchError((error) => {
      // Check if it is 401 Unauthorized and not already trying to refresh/login
      const isUnauthorized = error instanceof HttpErrorResponse && error.status === 401;
      const isAuthUrl = req.url.includes('/auth/login') || req.url.includes('/auth/refresh') || req.url.includes('/auth/logout');

      if (isUnauthorized && !isAuthUrl && authService.accessToken()) {
        console.warn(`[Auth Interceptor] Unauthorized request (401) on: ${req.url}. Initiating token refresh flow...`);

        if (!refreshToken$) {
          console.log('[Auth Interceptor] No active refresh session. Launching refreshAccessToken API call...');
          refreshToken$ = authService.refreshAccessToken().pipe(
            map(res => res.accessToken),
            shareReplay(1),
            finalize(() => {
              console.log('[Auth Interceptor] Refresh token request finalized. Resetting shared observable.');
              refreshToken$ = null;
            })
          );
        } else {
          console.log(`[Auth Interceptor] Token refresh is already in progress. Sharing active request: ${req.url}`);
        }

        return refreshToken$.pipe(
          switchMap((newAccessToken) => {
            console.log(`[Auth Interceptor] Retrying original request: ${req.url}`);
            const retryReq = req.clone({
              setHeaders: {
                Authorization: `Bearer ${newAccessToken}`
              }
            });
            return next(retryReq);
          }),
          catchError((refreshError) => {
            console.error('[Auth Interceptor] refreshAccessToken API failed. Forcing logout & session cleanup...', refreshError);
            authService.logoutLocal();
            router.navigate(['/auth/login']);
            return throwError(() => refreshError);
          })
        );
      }
      return throwError(() => error);
    })
  );
};
