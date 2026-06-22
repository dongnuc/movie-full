import { ApplicationConfig, provideZoneChangeDetection, APP_INITIALIZER } from '@angular/core';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { of, catchError } from 'rxjs';

import { routes } from './app.routes';
import { baseUrlInterceptor } from './core/interceptors/base-url.interceptor';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { AuthService } from './core/auth/auth.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([baseUrlInterceptor, errorInterceptor, authInterceptor])
    ),
    {
      provide: APP_INITIALIZER,
      useFactory: (authService: AuthService, router: Router) => () => {
        const hasToken = !!localStorage.getItem('access_token');
        const expiresAtStr = localStorage.getItem('access_token_expires_at');
        const isExpired = expiresAtStr ? new Date(expiresAtStr) <= new Date() : true;
        const isLoggedOut = localStorage.getItem('explicit_logout') === 'true';

        // If explicitly logged out, ensure local storage is fully cleared and do not refresh
        if (isLoggedOut) {
          authService.logoutLocal();
          return of(null);
        }

        // If there's no token or the token is expired, attempt token refresh on startup
        if (!hasToken || isExpired) {
          return authService.refreshAccessToken().pipe(
            catchError((err) => {
              console.warn('[APP_INITIALIZER] Startup token refresh failed:', err);
              authService.logoutLocal();
              if (hasToken) {
                router.navigate(['/auth/login']);
              }
              return of(null);
            })
          );
        }

        return of(null);
      },
      deps: [AuthService, Router],
      multi: true
    }
  ]
};
