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

        // Concurrency check: If the token has already changed while this request was in-flight,
        // it means another concurrent request has already refreshed the token.
        // We can just retry with the new token immediately.
        const currentToken = authService.accessToken();
        if (token && currentToken && token !== currentToken) {
          console.log(`[Auth Interceptor] Token was already refreshed from ${token.substring(0, 10)}... to ${currentToken.substring(0, 10)}... Retrying ${req.url} immediately.`);
          const retryReq = req.clone({
            setHeaders: {
              Authorization: `Bearer ${currentToken}`
            }
          });
          return next(retryReq);
        }

        if (!refreshToken$) {
          console.log('[Auth Interceptor] No active refresh session. Launching refreshAccessToken API call...');
          refreshToken$ = authService.refreshAccessToken().pipe(
            map(res => {
              if (!res || !res.accessToken) {
                throw new Error('No access token returned from refresh API');
              }
              return res.accessToken;
            }),
            shareReplay(1),
            catchError((refreshError) => {
              console.error('[Auth Interceptor] refreshAccessToken API failed. Forcing logout & session cleanup...', refreshError);
              authService.logoutLocal();
              router.navigate(['/auth/login']);
              return throwError(() => refreshError);
            }),
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
            console.log(`[Auth Interceptor] Retrying original request: ${req.url} with token: ${newAccessToken ? newAccessToken.substring(0, 15) : 'null'}...`);
            const retryReq = req.clone({
              setHeaders: {
                Authorization: `Bearer ${newAccessToken}`
              }
            });
            return next(retryReq).pipe(
              catchError((retryError) => {
                // If the retried request STILL fails with a 401 Unauthorized,
                // it means the newly refreshed token is also invalid/rejected.
                // In this specific case, force logout.
                if (retryError instanceof HttpErrorResponse && retryError.status === 401) {
                  console.error('[Auth Interceptor] Retried request still returned 401. Session is dead. Forcing logout...', retryError);
                  authService.logoutLocal();
                  router.navigate(['/auth/login']);
                }
                return throwError(() => retryError);
              })
            );
          })
        );
      }
      return throwError(() => error);
    })
  );
};
