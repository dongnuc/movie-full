import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult } from '../../shared/models/table-config';

export interface SeatType {
  id: string;
  name: string;
  surcharge: number;
  isActive: boolean;
  dateCreate?: string;
  dateUpdate?: string;
}

@Injectable({
  providedIn: 'root'
})
export class SeatTypeService {
  private http = inject(HttpClient);
  private readonly apiBase = '/api/v1/admin/seattypes';

  getActiveSeatTypes(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiBase}/active`);
  }

  getSeatTypes(page: number = 1, pageSize: number = 10, id?: string): Observable<PagedResult<SeatType>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (id) {
      params = params.set('id', id);
    }

    return this.http.get<PagedResult<SeatType>>(this.apiBase, { params });
  }

  getSeatTypeById(id: string): Observable<SeatType> {
    return this.http.get<SeatType>(`${this.apiBase}/${id}`);
  }

  createSeatType(seatType: Partial<SeatType>): Observable<any> {
    return this.http.post(this.apiBase, seatType);
  }

  updateSeatType(id: string, seatType: Partial<SeatType>): Observable<any> {
    return this.http.put(`${this.apiBase}/${id}`, seatType);
  }

  deleteSeatType(id: string): Observable<any> {
    return this.http.delete(`${this.apiBase}/${id}`);
  }
}
