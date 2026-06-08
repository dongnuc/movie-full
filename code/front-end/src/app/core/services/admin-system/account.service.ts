import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult } from '../../../shared/models/table-config';

export interface Account {
  id: string;
  username: string;
  email: string;
  role: 'SystemAdmin' | 'TheaterOwner' | 'User';
  isActive: boolean;
  dateCreate?: string;
  theaterIds?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private http = inject(HttpClient);
  private readonly apiBase = '/api/v1/admin/accounts';

  getAccounts(params: {
    search?: string;
    role?: string;
    page?: number;
    pageSize?: number;
  } = {}): Observable<PagedResult<Account>> {
    let httpParams = new HttpParams();

    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }
    if (params.role) {
      httpParams = httpParams.set('role', params.role);
    }
    if (params.page) {
      httpParams = httpParams.set('page', params.page.toString());
    }
    if (params.pageSize) {
      httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }

    return this.http.get<PagedResult<Account>>(this.apiBase, { params: httpParams });
  }

  toggleAccountStatus(id: string): Observable<any> {
    return this.http.delete(`${this.apiBase}/${id}`);
  }

  updateAccountRole(
    id: string,
    roleId: string,
    username: string,
    email: string,
    theaterIds?: string[]
  ): Observable<any> {
    return this.http.put(`${this.apiBase}/${id}`, {
      username,
      email,
      roleId,
      theaterIds: theaterIds || []
    });
  }

  createAccount(account: {
    username: string;
    email: string;
    roleId: string;
    password?: string;
    theaterIds?: string[];
  }): Observable<any> {
    return this.http.post(this.apiBase, account);
  }
}
