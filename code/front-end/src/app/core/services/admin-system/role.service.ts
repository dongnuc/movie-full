import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Role {
  id: string;
  name: string;
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

  getRoleById(id: string): Observable<Role> {
    return this.http.get<Role>(`${this.apiBase}/${id}`);
  }

  createRole(name: string): Observable<string> {
    return this.http.post<string>(this.apiBase, { name });
  }

  updateRole(id: string, name: string): Observable<void> {
    return this.http.put<void>(`${this.apiBase}/${id}`, { name });
  }

  deleteRole(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiBase}/${id}`);
  }
}
