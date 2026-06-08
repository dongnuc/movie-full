import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SeatService {
  private http = inject(HttpClient);
  private readonly apiBase = '/api/v1/admin/seats';

  bulkCreateSeats(roomId: string): Observable<any> {
    return this.http.post(`${this.apiBase}/bulk`, { roomId });
  }

  bulkUpdateSeatType(seatIds: string[], seatTypeId: string): Observable<any> {
    return this.http.put(`${this.apiBase}/seattype/bulk`, { seatIds, seatTypeId });
  }

  updateSeatsToCouple(seatIds: string[]): Observable<any> {
    return this.http.put(`${this.apiBase}/seattype/couple`, { seatIds });
  }
}
