import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult } from '../../../shared/models/table-config';

export interface TheaterMember {
  id: string;
  username: string;
  email: string;
  roles: string[];
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class TheaterMemberService {
  private http = inject(HttpClient);
  private readonly apiBase = '/api/v1/admin/theater/members';

  getMembers(params: {
    search?: string;
    page?: number;
    pageSize?: number;
  } = {}): Observable<PagedResult<TheaterMember>> {
    let httpParams = new HttpParams();

    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }
    if (params.page) {
      httpParams = httpParams.set('page', params.page.toString());
    }
    if (params.pageSize) {
      httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }

    return this.http.get<PagedResult<TheaterMember>>(this.apiBase, { params: httpParams });
  }

  addMember(username: string, roleId: string): Observable<any> {
    return this.http.post(this.apiBase, { username, roleId });
  }

  updateMember(id: string, roleId: string): Observable<any> {
    return this.http.put(`${this.apiBase}/${id}`, { roleId });
  }

  removeMember(id: string): Observable<any> {
    return this.http.delete(`${this.apiBase}/${id}`);
  }
}
