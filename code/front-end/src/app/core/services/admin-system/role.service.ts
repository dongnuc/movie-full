import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Role {
  id: string;
  name: string;
  permissions?: { [category: string]: number };
}

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  private http = inject(HttpClient);
  private readonly apiBase = '/api/v1/admin/roles';

  getRoles(): Observable<Role[]> {
    return this.http.get<Role[]>(this.apiBase);
  }

  getTheaterRoles(): Observable<Role[]> {
    return this.http.get<Role[]>('/api/v1/admin/theater/roles');
  }

  getSystemRoles(): Observable<Role[]> {
    return this.http.get<Role[]>('/api/v1/admin/system/roles');
  }

  getRoleById(id: string): Observable<Role> {
    return this.http.get<Role>(`${this.apiBase}/${id}`);
  }

  createRole(name: string, permissions?: { [category: string]: number }): Observable<string> {
    return this.http.post<string>(this.apiBase, { name, permissions });
  }

  updateRole(id: string, name: string, permissions?: { [category: string]: number }): Observable<void> {
    return this.http.put<void>(`${this.apiBase}/${id}`, { name, permissions });
  }

  deleteRole(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiBase}/${id}`);
  }
}
