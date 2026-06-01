import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult } from '../../shared/models/table-config';

export interface Showing {
  id: string;
  timeStart: string;
  price: number;
  maxTicketsPerUser: number;
  movieId: string;
  movieTitle?: string;
  movieDuration?: number;
  roomId: string;
  roomName?: string;
  theaterId?: string;
  isActive: boolean;
  dateCreate?: string;
  dateUpdate?: string;
}

export interface ShowingGroupedByMovie {
  movieId: string;
  movieTitle: string;
  movieImage: string;
  showings: Showing[];
}

@Injectable({
  providedIn: 'root'
})
export class ShowingService {
  private http = inject(HttpClient);
  private readonly apiBase = '/api/v1/admin/showings';

  getShowingsByTheater(theaterId: string, params: {
    dateStart?: string;
    pageIndex?: number;
    pageSize?: number;
    isActive?: boolean;
  } = {}): Observable<PagedResult<ShowingGroupedByMovie>> {
    let httpParams = new HttpParams();

    if (params.dateStart) httpParams = httpParams.set('dateStart', params.dateStart);
    if (params.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());

    const pageIndex = params.pageIndex || 1;
    const pageSize = params.pageSize || 10;
    httpParams = httpParams.set('pageIndex', pageIndex.toString());
    httpParams = httpParams.set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<ShowingGroupedByMovie>>(`${this.apiBase}/theater/${theaterId}`, { params: httpParams });
  }

  getShowingById(id: string): Observable<Showing> {
    return this.http.get<Showing>(`${this.apiBase}/${id}`);
  }

  createShowing(showing: Partial<Showing>): Observable<any> {
    return this.http.post(this.apiBase, showing);
  }

  updateShowing(id: string, showing: Partial<Showing>): Observable<any> {
    return this.http.put(`${this.apiBase}/${id}`, showing);
  }
}
