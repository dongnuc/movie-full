import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError, of, BehaviorSubject } from 'rxjs';

export interface UserInfo {
  id: string;
  username: string;
  email: string;
  roles: string[];
}

export interface LoginResponse {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken?: string;
}

export interface RefreshResponse {
  accessToken: string;
  accessTokenExpiresAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private readonly apiBase = '/api/v1/auth';

  // State Signals
  readonly accessToken = signal<string | null>(sessionStorage.getItem('access_token'));
  
  // Computed Signals
  readonly currentUser = computed<UserInfo | null>(() => {
    const token = this.accessToken();
    if (!token) return null;
    return this.decodeToken(token);
  });

  readonly isAuthenticated = computed(() => !!this.accessToken());
  
  readonly isSystemAdmin = computed(() => {
    const user = this.currentUser();
    return user ? user.roles.includes('SystemAdmin') : false;
  });

  readonly isTheaterOwner = computed(() => {
    const user = this.currentUser();
    return user ? user.roles.includes('TheaterOwner') : false;
  });

  // Track active token refreshing to avoid parallel duplicate refresh requests
  private isRefreshing = false;
  private refreshSubject = new BehaviorSubject<string | null>(null);

  login(usernameOrEmail: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiBase}/login`, {
      usernameOrEmail,
      password
    }).pipe(
      tap(res => this.setSession(res))
    );
  }

  register(username: string, email: string, password: string, confirmPassword: string): Observable<any> {
    return this.http.post(`${this.apiBase}/register`, {
      username,
      email,
      password,
      confirmPassword
    });
  }

  logout(): Observable<any> {
    return this.http.post(`${this.apiBase}/logout`, {}).pipe(
      tap(() => this.clearSession()),
      catchError(err => {
        this.clearSession();
        return throwError(() => err);
      })
    );
  }

  refreshAccessToken(): Observable<RefreshResponse> {
    return this.http.post<RefreshResponse>(`${this.apiBase}/refresh`, {}).pipe(
      tap(res => {
        sessionStorage.setItem('access_token', res.accessToken);
        sessionStorage.setItem('access_token_expires_at', res.accessTokenExpiresAt);
        this.accessToken.set(res.accessToken);
      }),
      catchError(err => {
        this.clearSession();
        return throwError(() => err);
      })
    );
  }

  private setSession(authResult: LoginResponse): void {
    sessionStorage.setItem('access_token', authResult.accessToken);
    sessionStorage.setItem('access_token_expires_at', authResult.accessTokenExpiresAt);
    this.accessToken.set(authResult.accessToken);
  }

  private clearSession(): void {
    sessionStorage.removeItem('access_token');
    sessionStorage.removeItem('access_token_expires_at');
    this.accessToken.set(null);
  }

  private decodeToken(token: string): UserInfo | null {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        window.atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      const payload = JSON.parse(jsonPayload);
      
      // Claims map to Microsoft Identity keys typically
      const rolesClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload['role'] || [];
      const roles = Array.isArray(rolesClaim) ? rolesClaim : [rolesClaim];

      return {
        id: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload['sub'],
        username: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || payload['unique_name'] || '',
        email: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || payload['email'] || '',
        roles: roles
      };
    } catch (e) {
      return null;
    }
  }
}
