import { ApplicationConfig, provideZoneChangeDetection, APP_INITIALIZER } from '@angular/core';
import { provideRouter } from '@angular/router';
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
      useFactory: (authService: AuthService) => () => {
        const hasToken = !!sessionStorage.getItem('access_token');
        const isLoggedOut = localStorage.getItem('explicit_logout') === 'true';
        if (!hasToken && !isLoggedOut) {
          return authService.refreshAccessToken().pipe(
            catchError(() => of(null))
          );
        }
        return of(null);
      },
      deps: [AuthService],
      multi: true
    }
  ]
};
