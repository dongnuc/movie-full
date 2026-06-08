import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult } from '../../../shared/models/table-config';

export interface Room {
  id: string;
  name: string;
  totalSeat: number;
  totalRow: number;
  matrix:string;
  totalCol: number;
  theaterId?: string;
  isActive: boolean;
  dateCreate?: string;
  dateUpdate?: string;
}

export interface SeatRowLayoutResponse {
  rowName: string;
  seats: any[];
}

export interface RoomLayoutResponse {
  id: string;
  name: string;
  totalSeat: number;
  totalRow: number;
  totalCol: number;
  isActive: boolean;
  seatRows: SeatRowLayoutResponse[];
}

@Injectable({
  providedIn: 'root'
})
export class RoomService {
  private http = inject(HttpClient);
  private readonly apiBase = '/api/v1/admin/rooms';

  getRooms(params: {
    search?: string;
    isActive?: boolean;
    page?: number;
    pageSize?: number;
  } = {}): Observable<PagedResult<Room>> {
    let httpParams = new HttpParams();

    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());

    const page = params.page || 1;
    const pageSize = params.pageSize || 10;
    httpParams = httpParams.set('page', page.toString());
    httpParams = httpParams.set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<Room>>(this.apiBase, { params: httpParams });
  }

  getRoomById(id: string): Observable<Room> {
    return this.http.get<Room>(`${this.apiBase}/${id}`);
  }

  createRoom(room: Partial<Room>): Observable<any> {
    // theaterId is omitted from the request body as it is read from JWT
    const { theaterId, ...rest } = room;
    return this.http.post(this.apiBase, rest);
  }

  updateRoom(id: string, room: Partial<Room>): Observable<any> {
    // theaterId is omitted from the request body as it is read from JWT
    const { theaterId, ...rest } = room;
    return this.http.put(`${this.apiBase}/${id}`, rest);
  }

  deleteRoom(id: string): Observable<any> {
    return this.http.delete(`${this.apiBase}/${id}`);
  }

  getRoomLayout(id: string): Observable<RoomLayoutResponse> {
    return this.http.get<RoomLayoutResponse>(`${this.apiBase}/${id}/layout`);
  }

  checkSeatsExist(id: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiBase}/${id}/seats/exists`);
  }

  getRoomsByTheater(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiBase}/theater`);
  }
}
