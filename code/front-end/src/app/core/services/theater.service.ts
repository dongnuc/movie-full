import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult } from '../../shared/models/table-config';

export interface Theater {
  id: string;
  name: string;
  location: string;
  isActive: boolean;
  dateCreate?: string;
  dateUpdate?: string;
}

@Injectable({
  providedIn: 'root'
})
export class TheaterService {
  private http = inject(HttpClient);
  private readonly apiBase = '/api/v1/admin/theaters';

  getTheaters(params: {
    search?: string;
    isActive?: boolean;
    page?: number;
    pageSize?: number;
  } = {}): Observable<PagedResult<Theater>> {
    let httpParams = new HttpParams();

    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());

    const page = params.page || 1;
    const pageSize = params.pageSize || 10;
    httpParams = httpParams.set('page', page.toString());
    httpParams = httpParams.set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<Theater>>(this.apiBase, { params: httpParams });
  }

  getTheaterById(id: string): Observable<Theater> {
    return this.http.get<Theater>(`${this.apiBase}/${id}`);
  }

  createTheater(theater: Partial<Theater>): Observable<any> {
    return this.http.post(this.apiBase, theater);
  }

  updateTheater(id: string, theater: Partial<Theater>): Observable<any> {
    return this.http.put(`${this.apiBase}/${id}`, theater);
  }

  deleteTheater(id: string): Observable<any> {
    return this.http.delete(`${this.apiBase}/${id}`);
  }
}
