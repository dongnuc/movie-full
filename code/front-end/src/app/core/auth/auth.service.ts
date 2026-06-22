import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
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
  readonly accessToken = signal<string | null>(localStorage.getItem('access_token'));
  
  // Computed Signals
  readonly currentUser = computed<UserInfo | null>(() => {
    const token = this.accessToken();
    if (!token) return null;
    return this.decodeToken(token);
  });

  readonly isAuthenticated = computed(() => !!this.accessToken());
  
  readonly isSystemAdmin = computed(() => {
    const user = this.currentUser();
    if (!user) return false;
    const systemAdminRoles = ['SystemAdmin', 'ADMIN-SYSTEM', 'ADMIN_SYSTEM', 'SYSTEM-ADMIN', 'SYSTEM_ADMIN'];
    return user.roles.some(role => 
      systemAdminRoles.some(adminRole => adminRole.toUpperCase() === role.toUpperCase())
    );
  });

  readonly isTheaterOwner = computed(() => {
    const user = this.currentUser();
    if (!user) return false;
    const theaterRoles = ['TheaterOwner', 'ADMIN-THEATER', 'ADMIN_THEATER', 'THEATER-OWNER', 'THEATER_OWNER', 'MANAGEMENT-THEATER', 'BOX-OFFICE-THEATER', 'BOX-OFFICE-STAFF'];
    return user.roles.some(role => 
      theaterRoles.some(theaterRole => theaterRole.toUpperCase() === role.toUpperCase())
    );
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

  resendConfirmEmail(email: string): Observable<any> {
    return this.http.post(`${this.apiBase}/resend-confirm-email`, { email });
  }

  confirmEmail(userId: string, token: string): Observable<any> {
    return this.http.post(`${this.apiBase}/confirm-email?userId=${userId}&token=${encodeURIComponent(token)}`, {});
  }

  logout(): Observable<any> {
    const token = this.accessToken();
    this.clearSession();

    let headers = new HttpHeaders();
    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }

    return this.http.post(`${this.apiBase}/logout`, {}, { headers }).pipe(
      catchError(err => {
        return throwError(() => err);
      })
    );
  }

  logoutLocal(): void {
    this.clearSession();
  }

  refreshAccessToken(): Observable<RefreshResponse> {
    return this.http.post<RefreshResponse>(`${this.apiBase}/refresh`, {}).pipe(
      tap(res => {
        console.log('[Auth Service] refreshAccessToken response success:', res);
        localStorage.setItem('access_token', res.accessToken);
        localStorage.setItem('access_token_expires_at', res.accessTokenExpiresAt);
        this.accessToken.set(res.accessToken);
      }),
      catchError(err => {
        console.error('[Auth Service] refreshAccessToken request failed:', err);
        this.clearSession();
        return throwError(() => err);
      })
    );
  }

  private setSession(authResult: LoginResponse): void {
    localStorage.setItem('access_token', authResult.accessToken);
    localStorage.setItem('access_token_expires_at', authResult.accessTokenExpiresAt);
    localStorage.removeItem('explicit_logout');
    this.accessToken.set(authResult.accessToken);
  }

  private clearSession(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('access_token_expires_at');
    localStorage.setItem('explicit_logout', 'true');
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
      
      // Try to get roles from the 'role' claim or keys of the serialized permissions JSON
      let roles: string[] = [];
      const directRole = payload['role'];
      if (directRole) {
        roles = Array.isArray(directRole) ? directRole : [directRole];
      }
      const rolesClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      if (rolesClaim) {
        if (typeof rolesClaim === 'string' && rolesClaim.trim().startsWith('{')) {
          try {
            const parsed = JSON.parse(rolesClaim);
            const keys = Object.keys(parsed);
            roles = [...roles, ...keys];
          } catch (e) {
            console.error('Error parsing permissions JSON in token:', e);
          }
        } else {
          const mappedRoles = Array.isArray(rolesClaim) ? rolesClaim : [rolesClaim];
          roles = [...roles, ...mappedRoles];
        }
      }
      roles = Array.from(new Set(roles));

      return {
        id: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload['sub'],
        username: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || payload['name'] || '',
        email: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || payload['email'] || '',
        roles: roles
      };
    } catch (e) {
      return null;
    }
  }
}
